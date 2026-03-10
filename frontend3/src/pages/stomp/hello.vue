<template>
  <div>
    <h1>Hello, Spring Boot!</h1>
  </div>
</template>

<script>
import stompjs from 'stompjs';
export default {
  name: 'Hello',
  mounted() {
    let url = 'ws://192.168.0.38:9030/stomp/websocket';
    let ws = stompjs.client(url);
    ws.connect({}, frame => {
      console.log('Connected: ' + frame);
      ws.subscribe('/topic/robot', message => {
        console.log('Received: ' + message.body);
      });
    }, error => {
      console.error('Connection error: ' + error);
    });
  }
};

</script>