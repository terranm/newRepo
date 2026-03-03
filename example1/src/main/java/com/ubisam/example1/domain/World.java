package com.ubisam.example1.domain;

import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.Entity;
// import jakarta.persistence.GeneratedValue;
// import jakarta.persistence.Id;
// import jakarta.persistence.Table;
import lombok.Data;

@Entity
@Data
public class World {
    @Id
    @GeneratedValue
    private Long id;
    private String name;

    @ManyToOne
    private Hello hello;
}
