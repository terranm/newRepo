package com.ubisam.example1;

// import java.util.List;
// import java.util.Optional;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;

import static io.u2ware.common.docs.MockMvcRestDocs.*;

import com.ubisam.example1.helloes.Hello;
import com.ubisam.example1.helloes.HelloRepository;

@SpringBootTest
@AutoConfigureMockMvc
class Example1ApplicationTests {
	@Autowired
	private HelloRepository helloRepository;

	@Autowired
	private MockMvc mockMvc;


	@Test
	void contextLoads() throws Exception {
		Hello h = new Hello();
		h.setName("name1");
		h.setEmail("a@mail.com");

		h = helloRepository.save(h); // c create	

		// 
		
		// // read
		// Optional<Hello> h2 = helloRepository.findById(1l);
		// System.out.println(h.getId());
		// System.out.println(h2.get().getId());

		// //update
		// h.setName("name2");
		// h = helloRepository.save(h);

		//delete
		//helloRepository.delete(h);

		//search
		// List<Hello> r = helloRepository.findAll();

		
	}

	
	@Test
	void contextLoads2() throws Exception {
		Hello h = new Hello();
		h.setName("name2");
		h.setEmail("b@mail.com");

		mockMvc.perform(post("/helloes").content(h)).andDo(print()).andExpect(is2xx());
		mockMvc.perform(get("/helloes")).andDo(print()).andExpect(is2xx());
	}

}
