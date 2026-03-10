<template>
  <div>
    <v-app-bar app color="yellow-darken-4" dark>
      <v-toolbar-title>Ubisam - yjk0827</v-toolbar-title>
      <v-spacer></v-spacer>
      <v-btn text @click="resetData">Reset</v-btn>
    </v-app-bar>

    <v-container>
      <v-row justify="center" class="mt-5">
        <v-col cols="8">
          <v-text-field
            v-model="searchText"
            label="Search"
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            @input="filterItems"
          >
            <template v-slot:append>
              <v-btn icon @click="openAddModal">
                <v-icon>mdi-plus</v-icon>
              </v-btn>
            </template>
          </v-text-field>
        </v-col>
      </v-row>

      <v-row justify="center">
        <v-col cols="10">
          <v-data-table
            :headers="headers"
            :items="filteredItems"
            class="elevation-1"
          >
            <template v-slot:item.actions="{ item }">
              <v-icon small class="mr-2" @click="openEditModal(item)">mdi-pencil</v-icon>
              <v-icon small @click="deleteItem(item)">mdi-delete</v-icon>
            </template>
          </v-data-table>
        </v-col>
      </v-row>
    </v-container>

    <!-- Add/Edit Modal -->
    <v-dialog v-model="dialog" max-width="500px">
      <v-card>
        <v-card-title>{{ isEdit ? 'Edit Item' : 'Add Item' }}</v-card-title>
        <v-card-text>
          <v-text-field v-model="currentItem.name" label="Name" required></v-text-field>
          <v-text-field v-model="currentItem.email" label="Email" required></v-text-field>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue darken-1" text @click="closeModal">Cancel</v-btn>
          <v-btn color="blue darken-1" text @click="saveItem">{{ isEdit ? 'Update' : 'Add' }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: 'Hello',
  data() {
    return {
      items: [],
      filteredItems: [],
      searchText: '',
      dialog: false,
      isEdit: false,
      currentItem: { name: '', email: '' },
      headers: [
        { text: 'ID', value: 'id' },
        { text: 'Name', value: 'name' },
        { text: 'Email', value: 'email' },
        { text: 'Actions', value: 'actions', sortable: false }
      ]
    };
  },
  methods: {
    loadData() {
      axios.get('http://localhost:8080/api/helloes')
        .then(res => {
          console.log('API Response:', res);
          this.items = res.data._embedded ? res.data._embedded.helloes : [];
          this.filteredItems = [...this.items];
          console.log('Items loaded:', this.items);
        })
        .catch(err => {
          console.error('Load data error', err);
        });
    },
    filterItems() {
      if (!this.searchText) {
        this.filteredItems = [...this.items];
      } else {
        this.filteredItems = this.items.filter(item =>
          item.name.toLowerCase().includes(this.searchText.toLowerCase()) ||
          item.email.toLowerCase().includes(this.searchText.toLowerCase())
        );
      }
    },
    openAddModal() {
      this.isEdit = false;
      this.currentItem = { name: '', email: '' };
      this.dialog = true;
    },
    openEditModal(item) {
      this.isEdit = true;
      this.currentItem = { ...item };
      this.dialog = true;
    },
    closeModal() {
      this.dialog = false;
    },
    saveItem() {
      if (this.isEdit) {
        axios.put(`http://localhost:8080/api/helloes/${this.currentItem.id}`, this.currentItem)
          .then(() => {
            this.loadData();
            this.closeModal();
          })
          .catch(err => {
            console.error('Update error', err);
          });
      } else {
        axios.post('http://localhost:8080/api/helloes', this.currentItem)
          .then(() => {
            this.loadData();
            this.closeModal();
          })
          .catch(err => {
            console.error('Add error', err);
          });
      }
    },
    deleteItem(item) {
      if (confirm('Are you sure you want to delete this item?')) {
        axios.delete(`http://localhost:8080/api/helloes/${item.id}`)
          .then(() => {
            this.loadData();
          })
          .catch(err => {
            console.error('Delete error', err);
          });
      }
    },
    resetData() {
      axios.delete('http://localhost:8080/api/helloes/reset')
        .then(() => {
          this.loadData();
        })
        .catch(err => {
          console.error('Reset error', err);
        });
    }
  },
  mounted() {
    this.loadData();
  }
};
</script>