<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'

// --- Interfaces to match backend DTOs ---
interface Suit {
  id: number
  name: string
  symbol: string
  colorHex: string
}

interface CardEffect {
  operation: string
  value: number
}

interface CardDTO {
  id: number
  rank: string
  suit: Suit
  effects: CardEffect[]
}

interface AvailableCardDTO {
  id: number
  card: CardDTO
}

// --- Component State ---
const deckCards = ref<AvailableCardDTO[]>([])
const allCards = ref<CardDTO[]>([])
const drawnCards = ref<CardDTO[]>([])
const cardSearchText = ref('') // Replaces selectedCardIdToAdd
const numberOfCardsToDraw = ref<number>(1)
const isLoading = ref(true)

const apiBaseUrl = '/api'

// --- Computed Properties ---
const deckSize = computed(() => deckCards.value.length)

// --- API Functions ---
async function fetchData() {
  isLoading.value = true
  try {
    const [deckResponse, cardsResponse] = await Promise.all([
      axios.get(`${apiBaseUrl}/deck`),
      axios.get(`${apiBaseUrl}/cards`)
    ])
    deckCards.value = deckResponse.data
    allCards.value = cardsResponse.data
  } catch (error) {
    console.error('Error fetching data:', error)
    alert('Failed to fetch deck data. Make sure the backend is running.')
  } finally {
    isLoading.value = false
  }
}

async function addCardToDeck() {
  if (!cardSearchText.value) {
    alert('Please select a card to add.')
    return
  }

  // Find the card object that matches the text input
  const selectedCard = allCards.value.find(
    (card) => formatCardName(card) === cardSearchText.value
  )

  if (!selectedCard) {
    alert('Card not found. Please select a valid card from the list.')
    return
  }

  try {
    await axios.post(`${apiBaseUrl}/deck`, { cardId: selectedCard.id })
    cardSearchText.value = '' // Clear the input field
    await fetchData() // Refresh the deck list
  } catch (error) {
    console.error('Error adding card to deck:', error)
    alert('Failed to add card.')
  }
}

async function drawCards() {
  if (numberOfCardsToDraw.value < 1) {
    alert('Number of cards to draw must be at least 1.')
    return
  }
  if (numberOfCardsToDraw.value > deckSize.value) {
    alert('Cannot draw more cards than are in the deck.')
    return
  }
  try {
    const response = await axios.post(`${apiBaseUrl}/deck/draw/${numberOfCardsToDraw.value}`)
    drawnCards.value = response.data
    await fetchData() // Refresh the deck list as drawing removes cards
  } catch (error) {
    console.error('Error drawing cards:', error)
    alert('Failed to draw cards.')
  }
}

// --- Helper Functions ---
function formatCardName(card: CardDTO): string {
  return `${card.rank} of ${card.suit.name} ${card.suit.symbol}`
}

onMounted(fetchData)
</script>

<template>
  <div class="deck-layout">
    <!-- Actions Panel -->
    <div class="actions-panel">
      <!-- Add Card -->
      <div class="action-card">
        <h2>Add Card to Deck</h2>
        <form @submit.prevent="addCardToDeck">
          <div class="form-group">
            <label for="card-select">Select Card</label>
            <input
              id="card-select"
              v-model="cardSearchText"
              type="text"
              list="card-list"
              placeholder="Search for a card..."
            />
            <datalist id="card-list">
              <option v-for="card in allCards" :key="card.id" :value="formatCardName(card)" />
            </datalist>
          </div>
          <button type="submit" class="btn btn-primary">Add Card</button>
        </form>
      </div>

      <!-- Draw Cards -->
      <div class="action-card">
        <h2>Draw Cards</h2>
        <form @submit.prevent="drawCards">
          <div class="form-group">
            <label for="draw-number">Number to Draw</label>
            <input
              id="draw-number"
              v-model.number="numberOfCardsToDraw"
              type="number"
              min="1"
              :max="deckSize"
            />
          </div>
          <button type="submit" class="btn btn-success" :disabled="deckSize === 0">Draw</button>
        </form>
      </div>

      <!-- Drawn Cards Display -->
      <div class="action-card">
        <h2>Last Draw</h2>
        <div v-if="drawnCards.length > 0" class="drawn-cards-list">
          <div v-for="card in drawnCards" :key="card.id" class="drawn-card">
            <span class="symbol" :style="{ color: card.suit.colorHex }">{{
              card.suit.symbol
            }}</span>
            <span>{{ card.rank }}</span>
          </div>
        </div>
        <p v-else>No cards drawn yet.</p>
      </div>
    </div>

    <!-- Deck List Panel -->
    <div class="deck-list-panel">
      <h2>Current Deck ({{ deckSize }} cards)</h2>
      <div v-if="isLoading">Loading deck...</div>
      <div v-else-if="deckCards.length === 0">The deck is empty.</div>
      <div v-else class="card-grid">
        <div v-for="item in deckCards" :key="item.id" class="deck-card">
          <div class="card-rank">{{ item.card.rank }}</div>
          <div class="card-symbol" :style="{ color: item.card.suit.colorHex }">
            {{ item.card.suit.symbol }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.deck-layout {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 2rem;
}

.actions-panel {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.action-card,
.deck-list-panel {
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

input[type='number'],
input[type='text'],
select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
}

.drawn-cards-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.drawn-card {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background-color: #fff;
  padding: 0.5rem;
  border-radius: 4px;
  border: 1px solid var(--color-border);
}

.drawn-card .symbol {
  font-size: 1.5rem;
}

.card-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 1rem;
}

.deck-card {
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 1rem;
  text-align: center;
  background-color: #fff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.card-rank {
  font-size: 1.2rem;
  font-weight: 600;
}

.card-symbol {
  font-size: 2.5rem;
  line-height: 1;
  margin-top: 0.5rem;
}

.btn {
  width: 100%;
  padding: 0.75rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
  font-weight: 600;
  color: white;
  transition: background-color 0.2s;
}

.btn:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.btn-primary {
  background-color: var(--color-accent);
}
.btn-primary:hover:not(:disabled) {
  background-color: var(--color-accent-hover);
}

.btn-success {
  background-color: #28a745;
}
.btn-success:hover:not(:disabled) {
  background-color: #218838;
}
</style>
