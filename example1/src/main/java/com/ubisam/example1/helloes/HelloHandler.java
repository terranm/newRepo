package com.ubisam.example1.helloes;

import org.springframework.data.rest.core.annotation.HandleAfterCreate;
import org.springframework.data.rest.core.annotation.HandleBeforeCreate;
import org.springframework.data.rest.core.annotation.RepositoryEventHandler;
import org.springframework.stereotype.Component;

@Component
@RepositoryEventHandler
public class HelloHandler {
    
    @HandleBeforeCreate
    public void beforeCreate(Hello hello){
        // hello.setName("1234");

        //throw new RuntimeException();
    }

    @HandleAfterCreate
    public void afterCreate(Hello hello){
        System.out.print("saramsalreo");
    }
}
