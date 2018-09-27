using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TourModels;
using TourService.BindingModels;
using TourService.Interfaces;
using TourService.ViewModels;

namespace TourService.Implementations
{
    public class JourneyService : IJourneyService
    {
        private ApplicationDbContext context;

        public JourneyService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public static JourneyService Create(ApplicationDbContext context)
        {
            return new JourneyService(context);
        }

        public async Task AddElement(JourneyBindingModel jModel, ReportBindingModel rModel)
        {
            if(jModel.TourJourneys.Count < 1)
            {
                throw new Exception("Отсутствуют туры");
            }
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var element = new Journey {
                        ClientId = jModel.ClientId,
                        DateCreate = DateTime.Now
                    };
                    context.Journeys.Add(element);
                    await context.SaveChangesAsync();

                    var groupTours = jModel.TourJourneys.GroupBy(rec => rec.TourId).Select(rec => new TourJourneyBindingModel
                    {
                        TourId = rec.Key,
                        Count = rec.Sum(r => r.Count)
                    });

                    foreach (var tour in groupTours)
                    {
                        context.TourJourneys.Add(new TourJourney
                        {
                            JourneyId = element.Id,
                            TourId = tour.TourId,
                            Count = tour.Count,
                            Price = context.Tours.FirstOrDefault(rec => rec.Id == tour.TourId).Price
                        });
                    }
                    await context.SaveChangesAsync();

                    var tours = context.Journeys.Where(rec=>rec.Id == element.Id).SelectMany(rec => rec.TourJourneys).Include(rec=>rec.Tour).Select(r => new TourViewModel
                    {
                        Name = r.Tour.Name,
                        Date = SqlFunctions.DateName("dd", r.Tour.Date) + " " +
                                            SqlFunctions.DateName("mm", r.Tour.Date) + " " +
                                            SqlFunctions.DateName("yyyy", r.Tour.Date),
                        Place = r.Tour.Place,
                        Price = r.Price,
                        Count = r.Count,
                        Total = r.Price * r.Count
                    }).ToList();
                    await SendJourneyReport(new JourneyViewModel
                    {
                        Date = DateTime.Now.ToLongDateString(),
                        Email = context.Users.FirstOrDefault(rec=>rec.Id == jModel.ClientId).UserName,
                        Tours = tours
                    }, rModel);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        private async Task SendJourneyReport(JourneyViewModel jModel, ReportBindingModel rModel)
        {
            rModel.FilePath += $@"{DateTime.Now.Ticks}.pdf";

            //открываем файл для работы
            FileStream fs = new FileStream(rModel.FilePath, FileMode.OpenOrCreate, FileAccess.Write);
            //создаем документ, задаем границы, связываем документ и поток
            iTextSharp.text.Document doc = new iTextSharp.text.Document();
            doc.SetMargins(0.5f, 0.5f, 0.5f, 0.5f);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            doc.Open();
            BaseFont baseFont = (rModel.FontPath != null && rModel.FontPath != string.Empty) ?
                BaseFont.CreateFont(rModel.FontPath, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED) : BaseFont.CreateFont();

            //вставляем заголовок
            var phraseTitle = new Phrase(string.Format("Ваше путешествие от {0}", jModel.Date),
                new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD));
            iTextSharp.text.Paragraph paragraph = new iTextSharp.text.Paragraph(phraseTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 12
            };
            doc.Add(paragraph);

            await AddJourneyToPdf(doc, baseFont, jModel);

            doc.Close();

            await SendMail(jModel.Email, "Ваше путешествие", "Вы только-что совершили заказ в нашем турагенстве", rModel.FilePath);

            File.Delete(rModel.FilePath);
        }

        public async Task SendJourneysReport(ReportBindingModel model)
        {

            var list = await GetJourneysReport(model);

            var client = await context.Users.FirstOrDefaultAsync(rec => rec.Id == model.ClientId);

            model.FilePath += $@"{DateTime.Now.Ticks}.pdf";

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
            var phraseTitle = new Phrase(string.Format("Отчет по путешествиям пользователя {0}", client.FIO),
                new iTextSharp.text.Font(baseFont, 16, iTextSharp.text.Font.BOLD));
            iTextSharp.text.Paragraph paragraph = new iTextSharp.text.Paragraph(phraseTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 12
            };
            doc.Add(paragraph);

            
            var phrasePeriod = new Phrase("c " + model.DateFrom.ToShortDateString() +
                                    " по " + model.DateTo.ToShortDateString(),
                                    new iTextSharp.text.Font(baseFont, 14, iTextSharp.text.Font.BOLD));
            paragraph = new iTextSharp.text.Paragraph(phrasePeriod)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 12
            };
            doc.Add(paragraph);

            foreach(var journey in list)
            {
                await AddJourneyToPdf(doc, baseFont, journey);
            }

            doc.Close();

            await SendMail(client.UserName, "Отчет по путешествиям", "Отчет по путешествиям c " + model.DateFrom.ToShortDateString() +
                                    " по " + model.DateTo.ToShortDateString(), model.FilePath);

            File.Delete(model.FilePath);
        }

        private Task AddJourneyToPdf(Document doc, BaseFont baseFont, JourneyViewModel model)
        {
            var phrazeJourney = new Phrase(string.Format("Путешествие от {0}", model.Date),
                                    new iTextSharp.text.Font(baseFont, 14, iTextSharp.text.Font.BOLD));
            var paragraph = new iTextSharp.text.Paragraph(phrazeJourney)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 12
            };
            doc.Add(paragraph);

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
            table.AddCell(new PdfPCell(new Phrase("Цена", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            table.AddCell(new PdfPCell(new Phrase("Количество", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            table.AddCell(new PdfPCell(new Phrase("Сумма", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            var fontForCells = new iTextSharp.text.Font(baseFont, 10);

            foreach (var tour in model.Tours)
            {
                cell = new PdfPCell(new Phrase(tour.Date, fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Name, fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Place, fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Price.ToString(), fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Count.ToString(), fontForCells));
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(tour.Total.ToString(), fontForCells));
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Phrase("Итого:", fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Colspan = 4,
                Border = 0
            };

            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(model.Tours.Select(rec => rec.Total).DefaultIfEmpty(0).Sum().ToString(), fontForCellBold))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Border = 0
            };
            table.AddCell(cell);

            doc.Add(table);
            return Task.CompletedTask;
        }

        private async Task<List<JourneyViewModel>> GetJourneysReport(ReportBindingModel model)
        {
            return await context.Journeys
                .Where(rec => rec.ClientId == model.ClientId)
                .Where(rec => rec.DateCreate >= model.DateFrom && rec.DateCreate <= model.DateTo)
                .Include(rec => rec.TourJourneys.Select(r => r.Tour))
                .Select(rec => new JourneyViewModel
                {
                    Date = SqlFunctions.DateName("dd", rec.DateCreate) + " " +
                                            SqlFunctions.DateName("mm", rec.DateCreate) + " " +
                                            SqlFunctions.DateName("yyyy", rec.DateCreate),
                    Tours = rec.TourJourneys.Select(r => new TourViewModel
                    {
                        Name = r.Tour.Name,
                        Date = SqlFunctions.DateName("dd", r.Tour.Date) + " " +
                                            SqlFunctions.DateName("mm", r.Tour.Date) + " " +
                                            SqlFunctions.DateName("yyyy", r.Tour.Date),
                        Place = r.Tour.Place,
                        Price = r.Price,
                        Count = r.Count,
                        Total = r.Price * r.Count
                    }).ToList()
                }).ToListAsync();
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
