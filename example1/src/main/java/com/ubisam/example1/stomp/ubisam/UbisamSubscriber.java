package com.ubisam.example1.stomp.ubisam;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ObjectNode;
import com.ubisam.example1.api.helloes.HelloRepository;
import com.ubisam.example1.domain.Hello;

import io.u2ware.common.stomp.client.WebsocketStompClient;
import io.u2ware.common.stomp.client.WebsocketStompClientHandler;
import io.u2ware.common.stomp.client.config.WebsocketStompProperties;

@Component
public class UbisamSubscriber implements WebsocketStompClientHandler {
    @Autowired
    public WebsocketStompProperties properties;
    
    public @Autowired ObjectMapper objectMapper;

    @Autowired
    private HelloRepository helloRepository;

    @Override
    public void handleFrame(WebsocketStompClient client, JsonNode message) {
        System.out.println("Received message: " + message.toString());
        ObjectNode data = objectMapper.createObjectNode();
        data.put("message", "Hello from Spring Boot! : " + message.toString());
        client.send("/app/robot", data);


        // 데이터 생성
        Hello h = new Hello();
        h.setName(message.toString());
        helloRepository.save(h);
    }

    @Override
    public String getDestination() {
        
        return properties.getSubscriptions().get("ubisam");
    }
    
}
