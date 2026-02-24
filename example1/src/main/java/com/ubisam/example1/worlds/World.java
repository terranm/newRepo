<<<<<<< HEAD
package com.ubisam.example1.worlds;

import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
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
}
=======
package com.ubisam.example1.worlds;

import jakarta.persistence.GeneratedValue;
import jakarta.persistence.Id;
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
}
>>>>>>> 246f19b90b35d326c94daf439787127f75fd4813
