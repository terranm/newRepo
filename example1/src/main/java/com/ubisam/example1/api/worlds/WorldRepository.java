package com.ubisam.example1.api.worlds;

import org.springframework.data.jpa.repository.JpaRepository;

import com.ubisam.example1.domain.World;

public interface WorldRepository extends JpaRepository<World, Long> {
    
}
