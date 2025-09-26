<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'

// --- Interfaces to match backend DTOs ---
interface Suit {
  id: number
  name: string
  symbol: string
}

// Enum now uses full names, which will be sent to the backend
enum EffectOperation {
  Add = 'Add',
  Subtract = 'Subtract',
  Multiply = 'Multiply',
  Divide = 'Divide',
  Modulo = 'Modulo',
  Power = 'Power',
  Root = 'Root',
  Log = 'Log',
  Factorial = 'Factorial',
  RandomAdd = 'RandomAdd',
  RandomMultiply = 'RandomMultiply',
  Ceiling = 'Ceiling',
  Floor = 'Floor'
}

interface CardEffect {
  operation: EffectOperation
  value: number
}

// Corrected interface to use camelCase to match typical JSON response
interface Card {
  id: number
  rank: string
  suit: Suit
  effects: CardEffect[]
}

// --- Component State ---
const cards = ref<Card[]>([])
const suits = ref<Suit[]>([])
const newCard = ref<{ rank: string; suitId: number | null; effects: CardEffect[] }>({
  rank: '',
  suitId: null,
  effects: []
})
const editingCard = ref<any | null>(null)
// The 'value' now sends the full enum name (e.g., "Add")
const effectOperations = [
  { value: EffectOperation.Add, text: 'Add (+)' },
  { value: EffectOperation.Subtract, text: 'Subtract (-)' },
  { value: EffectOperation.Multiply, text: 'Multiply (*)' },
  { value: EffectOperation.Divide, text: 'Divide (/)' },
  { value: EffectOperation.Modulo, text: 'Modulo (%)' },
  { value: EffectOperation.Power, text: 'Power (^)' },
  { value: EffectOperation.Root, text: 'Root (rt)' },
  { value: EffectOperation.Log, text: 'Log (log)' },
  { value: EffectOperation.Factorial, text: 'Factorial (!)' },
  { value: EffectOperation.RandomAdd, text: 'Random Add (rnd+)' },
  { value: EffectOperation.RandomMultiply, text: 'Random Multiply (rnd*)' },
  { value: EffectOperation.Ceiling, text: 'Ceiling (ceil)' },
  { value: EffectOperation.Floor, text: 'Floor (floor)' }
]

const apiBaseUrl = '/api'

// --- API Functions ---
async function fetchData() {
  try {
    const [cardsResponse, suitsResponse] = await Promise.all([
      axios.get(`${apiBaseUrl}/cards`),
      axios.get(`${apiBaseUrl}/suits`)
    ])
    cards.value = cardsResponse.data
    suits.value = suitsResponse.data
    if (suits.value.length > 0 && newCard.value.suitId === null) {
      newCard.value.suitId = suits.value[0].id // Default to the first suit
    }
  } catch (error) {
    console.error('Error fetching data:', error)
    alert('Failed to fetch data. Make sure the backend is running.')
  }
}

async function createCard() {
  if (!newCard.value.rank || !newCard.value.suitId) {
    alert('Card Rank and Suit are required.')
    return
  }
  try {
    await axios.post(`${apiBaseUrl}/cards`, newCard.value)
    newCard.value = { rank: '', suitId: suits.value[0]?.id || null, effects: [] }
    await fetchData()
  } catch (error) {
    console.error('Error creating card:', error)
    alert('Failed to create card.')
  }
}

async function deleteCard(cardId: number) {
  if (confirm('Are you sure you want to delete this card?')) {
    try {
      await axios.delete(`${apiBaseUrl}/cards/${cardId}`)
      await fetchData()
    } catch (error) {
      console.error('Error deleting card:', error)
      alert('Failed to delete card.')
    }
  }
}

function startEditing(card: Card) {
  editingCard.value = { ...card, suitId: card.suit.id } // Copy and add suitId for the form
}

async function updateCard() {
  if (!editingCard.value) return
  try {
    const { id, rank, suitId, effects } = editingCard.value
    await axios.put(`${apiBaseUrl}/cards/${id}`, { rank, suitId, effects })
    editingCard.value = null
    await fetchData()
  } catch (error) {
    console.error('Error updating card:', error)
    alert('Failed to update card.')
  }
}

function cancelEditing() {
  editingCard.value = null
}

// --- Helper Functions ---
function addEffect(target: 'new' | 'edit') {
  const effect = { operation: EffectOperation.Add, value: 0 }
  if (target === 'new') {
    newCard.value.effects.push(effect)
  } else if (editingCard.value) {
    editingCard.value.effects.push(effect)
  }
}

function removeEffect(target: 'new' | 'edit', index: number) {
  if (target === 'new') {
    newCard.value.effects.splice(index, 1)
  } else if (editingCard.value) {
    editingCard.value.effects.splice(index, 1)
  }
}

function formatEffects(effects: CardEffect[]): string {
  if (!effects || effects.length === 0) return 'None'
  return effects.map((e) => `${e.operation}: ${e.value}`).join(', ')
}

onMounted(fetchData)
</script>

<template>
  <div class="cards-container">
    <!-- Create Card Form -->
    <div class="form-card">
      <h2>Create New Card</h2>
      <form @submit.prevent="createCard">
        <div class="form-group">
          <label for="rank">Rank</label>
          <input id="rank" v-model="newCard.rank" type="text" placeholder="e.g., Ace" required />
        </div>
        <div class="form-group">
          <label for="suit">Suit</label>
          <select id="suit" v-model="newCard.suitId" required>
            <option v-for="suit in suits" :key="suit.id" :value="suit.id">
              {{ suit.name }} ({{ suit.symbol }})
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>Effects</label>
          <div v-for="(effect, index) in newCard.effects" :key="index" class="effect-editor">
            <select v-model="effect.operation">
              <option v-for="op in effectOperations" :key="op.value" :value="op.value">
                {{ op.text }}
              </option>
            </select>
            <input v-model.number="effect.value" type="number" step="any" />
            <button type="button" @click="removeEffect('new', index)" class="btn btn-danger btn-sm">
              Remove
            </button>
          </div>
          <button type="button" @click="addEffect('new')" class="btn btn-secondary btn-sm">
            Add Effect
          </button>
        </div>
        <button type="submit" class="btn btn-primary">Create Card</button>
      </form>
    </div>

    <!-- Cards List -->
    <div class="list-card">
      <h2>Existing Cards</h2>
      <table>
        <thead>
          <tr>
            <th>Rank</th>
            <th>Suit</th>
            <th>Effects</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="card in cards" :key="card.id">
            <template v-if="editingCard && editingCard.id === card.id">
              <!-- Editing View -->
              <td><input v-model="editingCard.rank" type="text" /></td>
              <td>
                <select v-model="editingCard.suitId">
                  <option v-for="suit in suits" :key="suit.id" :value="suit.id">
                    {{ suit.name }}
                  </option>
                </select>
              </td>
              <td>
                <div
                  v-for="(effect, index) in editingCard.effects"
                  :key="index"
                  class="effect-editor"
                >
                  <select v-model="effect.operation">
                    <option v-for="op in effectOperations" :key="op.value" :value="op.value">
                      {{ op.text }}
                    </option>
                  </select>
                  <input v-model.number="effect.value" type="number" step="any" />
                  <button
                    type="button"
                    @click="removeEffect('edit', index)"
                    class="btn btn-danger btn-sm"
                  >
                    &times;
                  </button>
                </div>
                <button
                  type="button"
                  @click="addEffect('edit')"
                  class="btn btn-secondary btn-sm"
                >
                  +
                </button>
              </td>
              <td class="actions">
                <button @click="updateCard" class="btn btn-success">Save</button>
                <button @click="cancelEditing" class="btn btn-secondary">Cancel</button>
              </td>
            </template>
            <template v-else>
              <!-- Default View -->
              <td>{{ card.rank }}</td>
              <td>{{ card.suit.name }} {{ card.suit.symbol }}</td>
              <td>{{ formatEffects(card.effects) }}</td>
              <td class="actions">
                <button @click="startEditing(card)" class="btn btn-primary">Edit</button>
                <button @click="deleteCard(card.id)" class="btn btn-danger">Delete</button>
              </td>
            </template>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.cards-container {
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
input[type='text'],
input[type='number'],
select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
}
.effect-editor {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  margin-bottom: 0.5rem;
}
.effect-editor select {
  width: 40%;
}
.effect-editor input {
  width: 40%;
}
.effect-editor button {
  width: 20%;
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
.actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
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
.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.8rem;
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