package com.ubisam.example1.worlds;

import org.springframework.data.jpa.repository.JpaRepository;

public interface WorldRepository extends JpaRepository<World, Long> {
    
}
