const searchParams = (params) => {
		return Object.keys(params).map((key) => {
     		return encodeURIComponent(key) + '=' + encodeURIComponent(params[key]);
   		})
		.join('&');
	};

const postRequest = async(params) => {
	return await fetch(params.url,{
					method: "POST",
					headers: {
						'Authorization': params.token,
						'Content-Type': 'application/json'
					},
					body: JSON.stringify(params.content)
				});
};

const getRequest = async(params) => {
	return await fetch(params.url,{
					method: "GET",
					headers: {
						'Authorization': params.token
					},
				});
};

	const tabs = {
		login: 'login',
		regist: 'regist',
		container: 'container',
		grid: 'grid',
		report: 'report'
	};

	const globalData = {
		token: null
	}

	globalData.install = function(){
		Object.defineProperty(Vue.prototype, '$globalData', {
    get () { return globalData },
  })
	}

	Vue.use(globalData);

	Vue.component('app-container', {
		props: {
			token: String
		},
		template: `
		<div>
			<nav class="navbar navbar-expand-lg navbar-light bg-light">
				<a v-for="(item, index) in head" class="navbar-brand" href="#" @click.prevent="setTab(item.tab)" @mouseover="highlight(index)" @mouseleave="highlight(index)"
				:style="(item.hover) ? {color: '#A3A1F3'} : {}">{{item.title}}</a>
			</nav>
			<keep-alive>
			<component v-bind:is="currentTabApp" class="app" @request="request($event)" @showmodal="$emit('showmodal',$event)"></component>
			</keep-alive>
		</div>
		`,
		data(){
			return {
				head: [
					{
						title: 'Туры',
						hover: false,
						tab: function(){
							this.appTab = tabs.grid
						}
					},
					{
						title: 'Получить отчет',
						hover: false,
						tab: function(){
							this.appTab = tabs.report
						}
					},
					{
						title: 'Выйти',
						hover: false,
						tab: function(){
							this.logout();
						}
					},
				],
				appTab: 'grid',
			};
		}, 
		methods: {
			highlight(index){
				this.head[index].hover = !this.head[index].hover;
			},
			setTab(func){
				func.apply(this);
			}, 
			logout(){
				this.$emit('logout');
			},
			
		},
		computed: {
			currentTabApp(){
  				return 'app-' + this.appTab.toLowerCase();
  			}
		}
	})

	Vue.component('app-report',{
		template:`
		<form class="input-form" @submit.prevent="send">
			<div>
	 		 	<div class="form-group" style="display: inline-block; margin: 0 auto;">
	 		 		<label>Дата начала:</label>
	 		 		<vuejs-datepicker v-model="dates.DateFrom" ></vuejs-datepicker>
	 		 	</div>
	 		 	<div class="form-group" style="display: inline-block; margin: 0 auto;">
	 		 		<label>Дата конца:</label>
	 		 		<vuejs-datepicker v-model="dates.DateTo" ></vuejs-datepicker>
	 		 	</div>
 		 	</div>
 		 	<br><br>
 		 	<button class="btn btn-success">Отправить</button>
 		 </form>
		`,
		data(){
			return{
				dates:{
					DateFrom: new Date(),
					DateTo: new Date()
				}
			}
		},
		methods: {
			send(){
				(async() => {
					if(this.dates.dateFrom > this.dates.dateTo){
						this.$emit('showmodal',{
								head: 'Ошибка',
								message: 'Дата начала должна быть меньше даты окончания!'
							});
						return;
					}
					const response = await postRequest({
							url: "api/journey/SendJourneysReport",
							token: this.$globalData.token,
							content: this.dates
						});
					if(response.ok){
						this.$emit('showmodal',{
							head: 'Успех',
							message: 'Отчет отправлен на почту.'
						});
					}else{
						const content = await response.json();
						this.$emit('showmodal',{
							head: 'Ошибка',
							message: content.error_description || content.error || content.ExceptionMessage
						});
					}
				})();
				
				
			}
		},
		components: {
  			vuejsDatepicker
  		},
  		activated(){
  			this.dates.DateFrom = new Date();
  			this.dates.DateTo = new Date();
  		}
	});

	Vue.component('app-input', {
			props: ['name', 'value', 'pattern','type'],
			template: `
				<div class="form-group">
                    <label>{{ name }}</label>
                    <span class="fa" :class="validClass" v-if="activated">
                    </span>
                    <input :type="type" 
                    	   class="form-control"
						   :value="value"
						   @input="onInput"
                    	   >
                </div>
			`,
			data(){
				return {
					activated: this.value != ''
				}
			},
			computed: {
				validClass(){
					return this.pattern.test(this.value) ? 
							'fa-check-circle text-success' :
							'fa-exclamation-circle text-danger';
				}
			},
			methods: {
				onInput(e){
					this.activated = true;

					this.$emit('changedata', {
						value: e.target.value,
						valid: this.pattern.test(e.target.value)
					});
				}
			}
		});

	Vue.component('app-regist', {
		template: `
			<form class="input-form" @submit.prevent="register">
			<app-input v-for="(item, index) in info"
							   :name="item.name"
							   :value="item.value"
							   :pattern="item.pattern"
							   :type="item.type"
							   :key="index"
							   @changedata="onChangeData(index, $event)"
					>
	                </app-input>
			<button class="btn btn-success" :disabled="done < info.length">Подтвердить</button>
			<button class="btn btn-success" @click.prevent="close">Отмена</button>
 		 </form>
		`,
		data(){
			return {
				info: [
					{
						name: 'ФИО',
						value: '',
						pattern: /^[a-zA-Z А-Яа-я]{2,30}$/,
						type: 'text'
					},
					{
						name: 'Email',
						value: '',
						pattern: /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/,
						type: 'text'
					},
					{
						name: 'Пароль',
						value: '',
						pattern: /^(?=.*[a-z])(?=.*[0-9])^[a-zA-Z0-9]{5,}$/,
						type: 'password'
					},
					{
						name: 'Подтверждение пароля',
						value: '',
						pattern: /^.*/,
						type: 'password'
					},
				],
				controls: [],
				done: 0
			};
		},
		created(){
				for(let i = 0; i < this.info.length; i++){
					this.controls.push(false);
				}
		},
		methods: {
			register(){
				(async () => {
					const response = await fetch("api/account/register",{
					method: 'POST',
					headers: {
						'Accept': 'application/json',
						'Content-Type': 'application/json'
					},
					body: JSON.stringify({ 
						FIO: this.info[0].value,
						UserName: this.info[1].value,
						PasswordHash: this.info[2].value
					})
				});
				await response;
				if(response.ok){
					this.$emit('showmodal',{
						head: 'Успех',
						message: 'Регистрация прошла успешно'
					});
					this.$emit('settab', { tab: tabs.login });
				}else{
					response.json().then(error => {
						this.$emit('showmodal',{
							head: 'Ошибка',
							message: [].join.call(error.ModelState[""],' ')
						});
					});
				}
			})();
			},
			onChangeData(index, data){
					this.info[index].value = data.value;
					
					if(index == 2){
						if(data.value != ''){
							this.info[3].pattern = new RegExp('^' + this.info[2].value + '$');
							
							this.controls[3] = this.info[3].pattern.test(this.info[3].value);
						}
					}

					this.controls[index] = data.valid;

					let done = 0;

					for(let i = 0; i < this.controls.length; i++){
						if(this.controls[i]){
							done++;
						}
					}

					this.done = done;
			},
			close(){
				this.$emit('settab', {
					tab: tabs.login
				});
			}
		}
	})

	Vue.component('app-login',{
		template: `
			<form class="input-form" @submit.prevent="login">
 		 	<div class="form-group">
	            <label>Логин</label>
	            <input type="text" class="form-control" v-model="fields.username" @keyup.enter="submit" placeholder="login">
        	</div>
        	<div class="form-group">
	            <label>Пароль</label>
	            <input type="password" class="form-control" v-model="fields.password" @keyup.enter="submit" placeholder="password">
        	</div>
        	<button class="btn btn-success">Подтвердить</button>
        	<button class="btn btn-success" @click.prevent="regist">Зарегистрироваться</button>
 		 </form>
		`,
		data(){
			return{
				fields:{
					grant_type: 'password',
					username: '',
					password: ''
				}
			}
		},
		methods: {
			login(){
				(async () => {
					const response = await fetch("/token",{
						method: 'POST',
						headers: {
							'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8'
						},
						body: searchParams(this.fields)
					});
					const content = await response.json();
					
					if(content.error || content.error_description){
						this.$emit('showmodal',{
							head: 'Ошибка',
							message: content.error_description || content.error
						});
					}else{
						this.$emit('login',{
							token: content.token_type + ' ' + content.access_token
						});
					}
				})();
			},
			regist(){
				this.$emit('settab', { tab: tabs.regist });
			}

		},
		activated(){
			this.fields.username = '';
			this.fields.password = '';
		}
	});

	Vue.component('app-modal',{
		props:{
			modal: Object
		},
		template: '#modal-template'
			
	});
	Vue.component('app-tour-info',{
		props:{
			head: Array,
			item: Object
		},
		template: `
			<form>
				<div class="form-group" v-for="(field, key, index) in item">
					<label>{{head[index]}}:</label><br>
					<template>{{field}}</template>
				</div>
			</form>
		`
	});
	Vue.component('app-grid',{
		template:`
			<form @submit.prevent="confirm">
		<table class="table table-striped table-hover">
		<thead>
			<tr>
				<th>
					<label class="form-checkbox">
    <input type="checkbox" v-model="selectAll" @click="select">
    <i class="form-icon"></i>
  </label>
				</th>
				<th v-for="i in head">{{i}}</th>
			</tr>
		</thead>
		<tbody>
			<tr v-for="(value,index) in items">
				<td>
					<label class="form-checkbox">
    					<input type="checkbox" :value="value.Id" v-model="selected">
						<i class="form-icon"></i>
  					</label>
				</td>
				<td v-for="(e, key) in value" v-if="key != 'Id'">{{e}}</td>
			</tr>
		</tbody>
	</table>
	<hr>
	<button class="btn btn-success">Создать путешествие</button>
	<button class="btn btn-success" @click.prevent="send">Получить список на почту</button>
	</form>
		`,
		data(){
			return {
				head: ['Название','Дата','Место','Цена'],
				items: [],
				selected: [],
				selectAll: false
			};
		},
		methods: {
			select() {
				this.selected = [];
				if (!this.selectAll) {
					for (var i in this.items) {
						this.selected.push(this.items[i].Id);
					}

				}
			},
			confirm(){
				(async() => {
					if(this.selected.length < 1){
						this.$emit('showmodal',{
							head: 'Ошибка',
							message: "Выберите хотя бы один тур"
						});
						return;
					}
					const response = await postRequest({
						url: "api/journey/addelement",
						token: this.$globalData.token,
						content: this.selected.map((rec) => {
							return {
								TourId: rec,
								Count: 1
							};
						})
					});

					if(response.ok){
						this.$emit('showmodal',{
							head: 'Успех',
							message: 'Путешествие создано. Отчет отправлен на почту.'
						});
					}else{
						const content = await response.json();
						this.$emit('showmodal',{
							head: 'Ошибка',
							message: content.error_description || content.error || content.ExceptionMessage
						});
					}
				})();
				this.selected = [];
				this.getData();
			},
			getData(){
			(async() => {
				const response = await getRequest({
					url: "api/tour/getlist",
					token: this.$globalData.token
				});

				const content = await response.json();
				if(response.ok){
					this.items = content;
				}else{
					if(content.error || content.error_description || content.ExceptionMessage){
						this.$emit('showmodal',{
							head: 'Ошибка',
							message: content.error_description || content.error || content.ExceptionMessage
						});
					}
				}
			})();
			
			},
			send(){
				(async() => {
				const response = await postRequest({
					url: "api/tour/sendlist",
					token: this.$globalData.token
				});

				if(response.ok){
					this.$emit('showmodal',{
							head: 'Успех',
							message: 'Отчет отправлен на почту.'
						});
				}else{
					const content = await response.json();
					if(content.error || content.error_description || content.ExceptionMessage){
						this.$emit('showmodal',{
							head: 'Ошибка',
							message: content.error_description || content.error || content.ExceptionMessage
						});
					}
				}
			})();
			}
		},
		created(){
			this.getData();
		},
		activated(){
			this.selected = [];
			this.getData();
		}
	})

	new Vue({
	el: "#app",
	data: {
		currentTab: 'login',
		tabs: tabs,
		modal: {
			head: null,
			object: null,
			message: null
		},
		showModalFlag: false,
		token: null
	},
	methods: {
		login(info){
			this.$globalData.token = info.token;
			this.setTab(tabs.container);
		},
		logout(){
			this.$globalData.token = null;
			this.setTab(tabs.login);
		},
		setTab(tab){
			this.currentTab = tab;
		},
		showWindow(properties){
			this.modal = properties;
			this.showModalFlag = true;
		}
	},
	components: {
  		vuejsDatepicker
  	},
  	computed: {
  		currentTabComponent(){
  				return 'app-' + this.currentTab.toLowerCase();
  		},
  	}
});