package com.ubisam.example1.api.helloes;

import org.springframework.data.jpa.domain.Specification;
import org.springframework.data.rest.core.annotation.HandleAfterCreate;
import org.springframework.data.rest.core.annotation.HandleBeforeCreate;
import org.springframework.data.rest.core.annotation.RepositoryEventHandler;
import org.springframework.stereotype.Component;

import com.ubisam.example1.domain.Hello;

import io.u2ware.common.data.jpa.repository.query.JpaSpecificationBuilder;
import io.u2ware.common.data.rest.core.annotation.HandleBeforeRead;

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
        System.out.println("saramsalreo");
    }

    @HandleBeforeRead
    public void beforeSearch(Hello hello, Specification<Hello> spec){
        System.out.println("[BeforeSearch] " + hello.getKeyword());

        JpaSpecificationBuilder<Hello> query = JpaSpecificationBuilder.of(Hello.class);
        query.where()
            .and().eq("name", hello.getKeyword()).build(spec);
    }
}
