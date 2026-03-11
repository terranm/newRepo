<template>
  <div>
    <h1>Hello, Spring Boot!</h1>
    <div style="margin-top: 20px; padding: 10px; border: 1px solid #ccc;">
      <h3>수신된 로봇 데이터</h3>
      <ul>
        <li v-for="(msg, index) in messages" :key="index">
          {{ msg }}
        </li>
      </ul>
      <p v-if="messages.length === 0">아직 수신된 데이터가 없습니다.</p>
    </div>
  </div>
</template>

<script>
import stompjs from 'stompjs';
export default {
  name: 'Hello',
  data() {
    return {
      // 화면에 보여줄 메시지들을 저장할 빈 배열 선언
      messages: [] 
    };
  },
  mounted() {
    let url = 'ws://192.168.0.23:9030/stomp/websocket';
    let ws = stompjs.client(url);
    ws.connect({}, frame => {
      console.log('Connected: ' + frame);
      ws.subscribe('/topic/robot', message => {
        console.log('Received: ' + message.body);
        // 수신된 메시지를 Vue의 data(messages 배열)에 추가
        // 화살표 함수(Arrow Function)를 사용했기 때문에 this는 Vue 인스턴스를 가리킴
        this.messages.push(message.body);
      });
    }, error => {
      console.error('Connection error: ' + error);
    });
  }
};

</script>