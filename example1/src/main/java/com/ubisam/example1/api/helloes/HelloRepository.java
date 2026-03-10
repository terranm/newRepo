
package com.ubisam.example1.api.helloes;

import com.ubisam.example1.domain.Hello;

import io.u2ware.common.data.jpa.repository.RestfulJpaRepository;

// import com.ubisam.example1.helloes.Hello;

import java.util.List;


public interface HelloRepository extends  RestfulJpaRepository<Hello,Long> {//JpaRepository<Hello,Long> {
    List<Hello> findByEmail(String email);
    List<Hello> findByNameAndEmail(String name, String email);
    List<Hello> findByIdOrName(Long id, String name);
}
