using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourModels;
using TourService.Interfaces;
using TourService.ViewModels;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using TourService.BindingModels;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Net.Mail;
using System.Configuration;
using System.Net;

namespace TourService.Implementations
{
    public class TourService : ITourService
    {
        private ApplicationDbContext context;

        public TourService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public static TourService Create(ApplicationDbContext context)
        {
            return new TourService(context);
        }

        public async Task<TourViewModel> Get(int id)
        {
            Tour result = await context.Tours.FirstOrDefaultAsync(rec => rec.Id == id);

            if (result == null)
            {
                throw new Exception("Element not found");
            }

            return new TourViewModel
            {
                Id = result.Id,
                Date = result.Date.ToLongDateString(),
                Name = result.Name,
                Place = result.Place,
                Price = result.Price,
                Equipment = result.Equipment
            };
        }

        public async Task<List<TourViewModel>> GetList()
        {
            DateTime now = DateTime.Now;

            return await context.Tours.Where(rec => rec.Date > now && rec.Equipment).Select(rec => new TourViewModel
            {
                Id = rec.Id,
                Date = SqlFunctions.DateName("dd", rec.Date) + " " +
                                            SqlFunctions.DateName("mm", rec.Date) + " " +
                                            SqlFunctions.DateName("yyyy", rec.Date),
                Name = rec.Name,
                Place = rec.Place,
                Price = rec.Price,
                Equipment = rec.Equipment
            }).ToListAsync();
        }

        public async Task SendList(ReportBindingModel model)
        {
            model.FilePath += $@"{DateTime.Now.Ticks}.pdf";

            var now = DateTime.Now;

            //открываем файл для работы
            FileStream fs = new FileStream(model.FilePath, FileMode.OpenOrCreate, FileAccess.Write);
            //создаем документ, задаем границы, связываем документ и поток
            iTextSharp.text.Document doc = new iTextSharp.text.Document();
            doc.SetMargins(0.5f, 0.5f, 0.5f, 0.5f);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            doc.Open();
            BaseFont baseFont = (model.FontPath != null && model.FontPath != string.Empty) ?
                BaseFont.CreateFont(model.FontPath, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED) : BaseFont.CreateFont();

            //вставляем заголовок
            var phraseTitle = new Phrase(($"Список туров от {now.ToLongDateString()}"),
                new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD));
            iTextSharp.text.Paragraph paragraph = new iTextSharp.text.Paragraph(phraseTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 12
            };
            doc.Add(paragraph);

            var list = await context.Tours.Where(rec => rec.Date >= now).Select(rec => new TourViewModel
            {
                Id = rec.Id,
                Date = SqlFunctions.DateName("dd", rec.Date) + " " +
                                              SqlFunctions.DateName("mm", rec.Date) + " " +
                                              SqlFunctions.DateName("yyyy", rec.Date),
                Name = rec.Name,
                Place = rec.Place,
                Price = rec.Price,
                Equipment = rec.Equipment
            }).ToListAsync();

            PdfPTable table = new PdfPTable(6)
            {
                TotalWidth = 800F
            };

            PdfPCell cell = new PdfPCell();
            var fontForCellBold = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.BOLD);
            table.AddCell(new PdfPCell(new Phrase("Дата", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            table.AddCell(new PdfPCell(new Phrase("Название", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            table.AddCell(new PdfPCell(new Phrase("Локация", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            table.AddCell(new PdfPCell(new Phrase("Снаряжение", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            table.AddCell(new PdfPCell(new Phrase("Цена", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            var fontForCells = new iTextSharp.text.Font(baseFont, 10);

            foreach (var tour in list)
            {
                cell = new PdfPCell(new Phrase(tour.Date, fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Name, fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Place, fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Price.ToString(), fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Equipment ? "есть" : "нет", fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Price.ToString(), fontForCells));
                table.AddCell(cell);
            }

            doc.Add(table);

            doc.Close();

            await SendMail((await context.Users.FirstOrDefaultAsync(rec => rec.Id == model.ClientId)).UserName, "Список доступных туров",
                "Список доступных туров от " + now.ToShortDateString(), model.FilePath);

            File.Delete(model.FilePath);
        }

        public async System.Threading.Tasks.Task SendMail(string mailto, string caption, string message, string path = null)
        {
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
            SmtpClient stmpClient = null;
            try
            {
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["MailLogin"]);
                mailMessage.To.Add(new MailAddress(mailto));
                mailMessage.Subject = caption;
                mailMessage.Body = message;
                mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                if (path != null)
                {
                    mailMessage.Attachments.Add(new System.Net.Mail.Attachment(path));
                }
                mailMessage.IsBodyHtml = true;

                stmpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MailLogin"].Split('@')[0],
                    ConfigurationManager.AppSettings["MailPassword"])
                };
                await stmpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                mailMessage.Dispose();
                mailMessage = null;
                stmpClient = null;
            }
        }
    }
}
