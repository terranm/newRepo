package com.ubisam.example1.domain;

// import jakarta.persistence.Embeddable;
// import jakarta.persistence.EmbeddedId;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;
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

    @Transient // db에 저장되지 않는 필드
    private String keyword;
    // @Data
    // @Embeddable
    // public class Id{
    //     private String id1;
    //     private String id2;
    // }
}
