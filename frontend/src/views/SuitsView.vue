<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'

// Interface matching the backend SuitDTO
interface Suit {
  id: number
  name: string
  symbol: string
  colorHex: string
}

const suits = ref<Suit[]>([])
// Ref for creating a new suit, matching CreateSuitDTO
const newSuit = ref({ name: '', symbol: '', colorHex: '#E53935' })
// Ref for the suit currently being edited
const editingSuit = ref<Suit | null>(null)

const apiBaseUrl = '/api' // Using a relative URL for the proxy

// Fetch all suits
async function fetchSuits() {
  try {
    const response = await axios.get(`${apiBaseUrl}/suits`)
    suits.value = response.data
  } catch (error) {
    console.error('Error fetching suits:', error)
    alert('Failed to fetch suits. Make sure the backend is running.')
  }
}

// Create a new suit
async function createSuit() {
  if (!newSuit.value.name || !newSuit.value.symbol) {
    alert('Suit name and symbol are required.')
    return
  }
  try {
    await axios.post(`${apiBaseUrl}/suits`, newSuit.value)
    newSuit.value = { name: '', symbol: '', colorHex: '#E53935' } // Reset form
    await fetchSuits() // Refresh the list
  } catch (error) {
    console.error('Error creating suit:', error)
    alert('Failed to create suit.')
  }
}

// Delete a suit
async function deleteSuit(suitId: number) {
  if (confirm('Are you sure you want to delete this suit?')) {
    try {
      await axios.delete(`${apiBaseUrl}/suits/${suitId}`)
      await fetchSuits() // Refresh the list
    } catch (error) {
      console.error('Error deleting suit:', error)
      alert('Failed to delete suit.')
    }
  }
}

// Start editing a suit
function startEditing(suit: Suit) {
  // Create a copy to avoid modifying the original object directly
  editingSuit.value = { ...suit }
}

// Update an existing suit
async function updateSuit() {
  if (!editingSuit.value) return
  try {
    // The PUT endpoint expects the UpdateSuitDTO structure
    const updatePayload = {
      name: editingSuit.value.name,
      symbol: editingSuit.value.symbol,
      colorHex: editingSuit.value.colorHex
    }
    await axios.put(`${apiBaseUrl}/suits/${editingSuit.value.id}`, updatePayload)
    editingSuit.value = null // Exit editing mode
    await fetchSuits() // Refresh the list
  } catch (error) {
    console.error('Error updating suit:', error)
    alert('Failed to update suit.')
  }
}

// Cancel editing
function cancelEditing() {
  editingSuit.value = null
}

// Fetch suits when the component is mounted
onMounted(fetchSuits)
</script>

<template>
  <div class="suits-container">
    <!-- Create Suit Form -->
    <div class="form-card">
      <h2>Create New Suit</h2>
      <form @submit.prevent="createSuit">
        <div class="form-group">
          <label for="name">Suit Name</label>
          <input id="name" v-model="newSuit.name" type="text" placeholder="e.g., Hearts" required />
        </div>
        <div class="form-group">
          <label for="symbol">Symbol</label>
          <input
            id="symbol"
            v-model="newSuit.symbol"
            type="text"
            placeholder="e.g., ♥"
            maxlength="1"
            required
          />
        </div>
        <div class="form-group">
          <label for="colorHex">Color</label>
          <div class="color-input">
            <input id="colorHex" v-model="newSuit.colorHex" type="color" />
            <span>{{ newSuit.colorHex }}</span>
          </div>
        </div>
        <button type="submit" class="btn btn-primary">Create Suit</button>
      </form>
    </div>

    <!-- Suits List -->
    <div class="list-card">
      <h2>Existing Suits</h2>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Symbol</th>
            <th>Color</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="suit in suits" :key="suit.id">
            <template v-if="editingSuit && editingSuit.id === suit.id">
              <!-- Editing View -->
              <td><input v-model="editingSuit.name" type="text" /></td>
              <td><input v-model="editingSuit.symbol" type="text" maxlength="1" /></td>
              <td><input v-model="editingSuit.colorHex" type="color" /></td>
              <td class="actions">
                <button @click="updateSuit" class="btn btn-success">Save</button>
                <button @click="cancelEditing" class="btn btn-secondary">Cancel</button>
              </td>
            </template>
            <template v-else>
              <!-- Default View -->
              <td>{{ suit.name }}</td>
              <td class="symbol">{{ suit.symbol }}</td>
              <td>
                <span class="color-swatch" :style="{ backgroundColor: suit.colorHex }"></span>
                {{ suit.colorHex }}
              </td>
              <td class="actions">
                <button @click="startEditing(suit)" class="btn btn-primary">Edit</button>
                <button @click="deleteSuit(suit.id)" class="btn btn-danger">Delete</button>
              </td>
            </template>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.suits-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.form-card,
.list-card {
  background-color: #f8f9fa;
  padding: 1.5rem;
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

h2 {
  margin-top: 0;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid var(--color-border);
  padding-bottom: 0.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
}

input[type='text'] {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  font-size: 1rem;
}

.color-input {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

input[type='color'] {
  width: 40px;
  height: 40px;
  border: none;
  padding: 0;
  border-radius: 4px;
  cursor: pointer;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  text-align: left;
  padding: 0.75rem;
  border-bottom: 1px solid var(--color-border);
  vertical-align: middle;
}

th {
  font-weight: 600;
}

.symbol {
  font-size: 1.5rem;
}

.color-swatch {
  display: inline-block;
  width: 20px;
  height: 20px;
  border-radius: 4px;
  border: 1px solid var(--color-border);
  vertical-align: middle;
  margin-right: 0.5rem;
}

.actions {
  display: flex;
  gap: 0.5rem;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  font-weight: 600;
  color: white;
  transition: background-color 0.2s;
}

.btn-primary {
  background-color: var(--color-accent);
}
.btn-primary:hover {
  background-color: var(--color-accent-hover);
}

.btn-danger {
  background-color: #dc3545;
}
.btn-danger:hover {
  background-color: #c82333;
}

.btn-success {
  background-color: #28a745;
}
.btn-success:hover {
  background-color: #218838;
}

.btn-secondary {
  background-color: #6c757d;
}
.btn-secondary:hover {
  background-color: #5a6268;
}
</style>
