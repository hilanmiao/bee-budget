<template>
	<view>
		<view class="m-keyboard">
			<view class="fingerboard" :class="{isDecimal_:!isDecimal}">
				<view class="icon_ fingerboard-item"
					:class="[`fingerboard-item-${index}`,item.name == 'submit' ? isCheck ? 'fingerboard_item_color' : 'fingerboard_item_color_' : '']"
					hover-class="hover_class" :hover-stay-time="50" v-for="(item, index) in fingerboardList"
					:key="index" @click="Keyboard(item.name)">
					<view class="fingerboard-img" v-if="item.name === 'x'">
						<image class="img"
							src="data:image/svg+xml;base64,PHN2ZyBjbGFzcz0iaWNvbiIgdmlld0JveD0iMCAwIDEzNTUgMTAyNCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCI+PHBhdGggZD0iTTQxMC4xNjYgMTIxLjQ2N2g3NzUuMzIzdjc4MS4wNjZINDEwLjE2NkwxNTkuOTk1IDUxMmwyNTAuMTctMzkwLjUzM3ptNTUzLjM2MiAxOTQuNzM4Yy04LjcwNy0xMS42OTMtMjguOTkxLTExLjY5My0zNy42OTggMEw3NjkuMTU3IDQ3NC4wMjZsLTE1Ni42MjYtMTU3LjgyYy04LjcwNy0xMS42OTQtMjYuMTItOC43NzYtMzcuNzIxIDAtMTEuNjAxIDguNzc1LTExLjYwMSAyNi4zMDMgMCAzNy45OTZMNzMxLjQ4MyA1MTIgNTc0LjgxIDY2OS44MjFjLTExLjYwMSA4Ljc1My0xMS42MDEgMjYuMjggMCAzNy45NzQgOC43MyAxMS42OTMgMjYuMTIgMTEuNjkzIDM3LjcyIDBsMTU2LjY1LTE1Ny44MjEgMTU2LjY1IDE1Ny44MmM4LjcwNyAxMS42OTQgMjYuMDk3IDExLjY5NCAzNy42OTggMCAxMS42MDEtOC43NzUgMTEuNjAxLTI2LjMwMyAwLTM3Ljk5Nkw4MDYuOTAxIDUxMmwxNTYuNjczLTE1Ny44MjFjMTEuNTc4LTguNzUzIDExLjU3OC0yNi4yOCAwLTM3Ljk3NHoiIGRhdGEtc3BtLWFuY2hvci1pZD0iYTMxM3guc2VhcmNoX2luZGV4LjAuaTAuMTU2NjNhODFoUEZCc0EiIGNsYXNzPSJzZWxlY3RlZCIvPjwvc3ZnPg=="
							mode="aspectFill"></image>
					</view>
					<view v-else-if="item.name === 'submit'">{{submitButtonText}}</view>
					<view v-else>{{ item.name }}</view>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
	let enterAmount = '';
	export default {
		props: {
			// 是否可以输入小数
			isDecimal: {
				type: Boolean,
				default: true
			},
			// 提交按钮文本
			submitButtonText: {
				type: String,
				default: '确定'
			},
			/**
			 * 保留小数，最大3，
			 */
			toFixed: {
				type: [String, Number],
				default: 2
			},
			/**
			 * 输入最大值
			 */
			maxValue: {
				type: [String, Number],
				default: 100000000 //1亿
			},
			// 提交按钮是否通过校验
			isCheck:{
				type: Boolean,
				default: true
			}
		},
		data() {
			return {
				fingerboardList: [{
						name: '1'
					},
					{
						name: '2'
					},
					{
						name: '3'
					},
					{
						name: 'x'
					},
					{
						name: '4'
					},
					{
						name: '5'
					},
					{
						name: '6'
					},
					{
						name: 'submit'
					},
					{
						name: '7'
					},
					{
						name: '8'
					},
					{
						name: '9'
					},
					{
						name: '0'
					},
					{
						name: '.'
					}
				]
			};
		},
		watch: {
			isDecimal: {
				handler: function(newV) {
					if (!newV) {
						this.fingerboardList.pop()
					}
				},
				immediate: true
			}
		},
		created() {
			enterAmount = ''
		},
		methods: {
			/**
			 * 调用此方法直接修改enterAmount的值，同时会回调
			 * 使用场景：给输入框默认值时
			 */
			setKeyboard(e) {
				enterAmount = `${e}`
				const value = this.setValue()
				this.$emit('onChange', value)
			},
			Keyboard(key) {
				let lastIndexOf = enterAmount.lastIndexOf('.');
				switch (key) {
					case 'x':
						if (!enterAmount.length) return; //已经没有数了
						enterAmount = enterAmount.substr(0, enterAmount.length - 1);
						if (enterAmount[enterAmount.length - 1] === '.') { //如果删掉的前一个是点.那么就把点也删掉
							enterAmount = enterAmount.substr(0, enterAmount.length - 1);
						}
						break;
					case '.':
						if (lastIndexOf > 0 && key === '.') return; //只能有一个点.
						if (!enterAmount.length && key === '.') return; //第一个数不能是点.
						enterAmount = `${enterAmount}${key}`;
						break;
					case 'submit':
						if (!enterAmount) return;
						break;
					default:
						if (lastIndexOf > 0 && enterAmount.length - lastIndexOf > Number(this.toFixed))
							return; //只能有toFixed位小数

						// if ((enterAmount[0] === '0' && key === '0') && (enterAmount[1] !== '.')) return; //不可以是00
						//第一个是0 后面一个就必须是.
						if ((enterAmount[0] === '0' && enterAmount.length === 1) && key != '.') return;


						if (Number(`${enterAmount}${key}`) > Number(this.maxValue)) return this.$emit('onError','maxValue'), console.warn(
							`输入不通过，大于了maxValue的限定`);
						enterAmount = `${enterAmount}${key}`;
						break;
				};
				const value = this.setValue()
				if (key === 'submit') {
					if(this.isCheck){
						this.$emit('onSubmit', value)
					} else {
						this.$emit('onError','isCheck')
					}
				} else {
					this.$emit('onChange', value)
				}
			},
			setValue() {
				let numberEnterAmount = Number(enterAmount);
				numberEnterAmount = enterAmount === '' ? '' : numberEnterAmount
				if (enterAmount.slice(-3) === '.00') {
					enterAmount = `${enterAmount.slice(0,-3)}`
				}
				let value = {
					valueToLocaleString: numberEnterAmount.toLocaleString(),
					valueNumber: numberEnterAmount,
					enterAmount
				}
				// 最后一个是0
				if (enterAmount[enterAmount.length - 1] === '.') {
					value['valueToLocaleString'] = `${value['valueToLocaleString'] }.`
				}
				if (enterAmount.slice(-2) === '.0') {
					value['valueToLocaleString'] = `${value['valueToLocaleString'] }.0`
				}
				if (enterAmount === '0.0') {
					value['valueToLocaleString'] = `${enterAmount}`
				}
				return value;
			},
		}
	};
</script>

<style scoped lang="scss">
	.icon_ {
		display: flex;
		justify-content: center;
		align-items: center;
	}

	.hover_class::after {
		position: absolute;
		z-index: 2;
		content: '';
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		background-color: rgba(0, 0, 0, 0.1);
	}

	.m-keyboard {
		width: 100%;

		.fingerboard {
			width: 100%;
			display: grid;
			grid-template-columns: 1fr 1fr 1fr 1fr;
			grid-template-rows: 90rpx 90rpx 90rpx 90rpx;
			grid-gap: 8px;

			.fingerboard-item {
				position: relative;
				overflow: hidden;
				background-color: #fff;
				border-radius: 10rpx;
				text-align: center;
				line-height: 90rpx;
				color: #000;
				font-size: 36rpx;
				font-weight: bold;

				.delete {
					width: 60rpx;
					height: 60rpx;
				}
			}

			.fingerboard-item-7 {
				line-height: 310rpx;
				grid-column: 4 / 4;
				grid-row: 2 / 5;
			}

			.fingerboard-item-11 {
				grid-column: 1 / 3;
				grid-row: 4 / 4;
			}

			.fingerboard_item_color {
				//background-color: #0CC160;
				background-color: $theme-color-primary-brand;
				color: #fff;
			}
			.fingerboard_item_color_{
				opacity: 0.4;
				//background-color: #0CC160;
        background-color: $theme-color-primary-brand;
				color: #fff;
			}

			.hover-class {
				background-color: #dcdcdc;
			}

			.fingerboard-img {
				box-sizing: border-box;
				padding-right: 6rpx;
				width: 60rpx;
				height: 60rpx;

				.img {
					width: 100%;
					height: 100%;
				}
			}
		}

		.isDecimal_ {
			.fingerboard-item-11 {
				grid-column: 1 / 4;
				grid-row: 4 / 4;
			}
		}
	}
</style>