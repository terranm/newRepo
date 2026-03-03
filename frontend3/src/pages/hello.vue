<template>
Hello
</template>

<script>
import axios from "axios";
export default { 
    name: 'Hello',

    mounted() {
        console.log('Hello mounted');
        axios({
            method: 'post',
            url: 'http://localhost:8080/api/helloes/search',
            data: {
                keyword: 'Hello'
            }
        })
        .then(res => {
            console.log('Hello axios response', res);
        })
        .catch(err => {
            console.error('Hello axios error', err);
        });
    }
}
</script>