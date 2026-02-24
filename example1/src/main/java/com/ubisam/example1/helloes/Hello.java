package com.ubisam.example1.helloes;

// import jakarta.persistence.Embeddable;
// import jakarta.persistence.EmbeddedId;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import lombok.Data;

// ORM // table 생성
@Entity
@Data
@Table(name="example_hello")
public class Hello {
    @Id
    @GeneratedValue
    private Long id; 
    // @EmbeddedId
    // private Id id;
    private String name;
    private String email;

    // @Data
    // @Embeddable
    // public class Id{
    //     private String id1;
    //     private String id2;
    // }
}
