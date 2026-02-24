<<<<<<< HEAD
package com.ubisam.example1.helloes;

import org.springframework.data.jpa.repository.JpaRepository;

// import com.ubisam.example1.helloes.Hello;

import java.util.List;


public interface HelloRepository extends JpaRepository<Hello,Long> {
    List<Hello> findByEmail(String email);
    List<Hello> findByNameAndEmail(String name, String email);
    List<Hello> findByIdOrName(Long id, String name);
}
=======
package com.ubisam.example1.helloes;

import org.springframework.data.jpa.repository.JpaRepository;

// import com.ubisam.example1.helloes.Hello;

import java.util.List;


public interface HelloRepository extends JpaRepository<Hello,Long> {
    List<Hello> findByEmail(String email);
    List<Hello> findByNameAndEmail(String name, String email);
    List<Hello> findByIdOrName(Long id, String name);
}
>>>>>>> 246f19b90b35d326c94daf439787127f75fd4813
