const ClickOutside = {

    beforeMount (el, binding) {

        el.clickOutsideEvent = (event) => {

            // 检查点击是否在el之外

            if (!(el).contains(event.target)) {

                binding.value(event)

            }

        }

        document.addEventListener('click', el.clickOutsideEvent)

    },

    unmounted (el) {

        document.removeEventListener('click', el.clickOutsideEvent)

    }

}

export default ClickOutside;
