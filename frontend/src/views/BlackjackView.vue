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

interface BlackjackGameState {
  gameId: number
  playerHand: CardDTO[]
  dealerHand: CardDTO[]
  playerScore: number
  dealerScore: number
  status: string // e.g., "PlayerTurn", "DealerTurn", "PlayerWins", "DealerWins", "Push"
  message: string
  playerMoney: number
  currentBet: number
}

// --- Component State ---
const gameId = ref<number | null>(null)
const gameState = ref<BlackjackGameState | null>(null)
const isLoading = ref(false)
const betAmount = ref<number>(10) // Default bet
const errorMessage = ref<string | null>(null)

const apiBaseUrl = '/api'

// --- Computed Properties ---
const isGameActive = computed(() => gameState.value?.status === 'PlayerTurn')
const canStartNewGame = computed(() => !gameId.value || !isGameActive.value)

// --- API Functions ---
async function startGame() {
  isLoading.value = true
  errorMessage.value = null
  try {
    const response = await axios.post(`${apiBaseUrl}/blackjack`, { bet: betAmount.value })
    gameId.value = response.data.gameId
    await fetchGameState()
  } catch (error: any) {
    console.error('Error starting game:', error)
    errorMessage.value = error.response?.data?.message || 'Failed to start game.'
  } finally {
    isLoading.value = false
  }
}

async function fetchGameState() {
  if (!gameId.value) return
  isLoading.value = true
  errorMessage.value = null
  try {
    const response = await axios.get(`${apiBaseUrl}/blackjack/${gameId.value}`)
    gameState.value = response.data
  } catch (error: any) {
    console.error('Error fetching game state:', error)
    errorMessage.value = error.response?.data?.message || 'Failed to fetch game state.'
    gameId.value = null // Invalidate game if state cannot be fetched
  } finally {
    isLoading.value = false
  }
}

async function playerAction(action: 'hit' | 'stand') {
  if (!gameId.value || !isGameActive.value) return
  isLoading.value = true
  errorMessage.value = null
  try {
    await axios.post(`${apiBaseUrl}/blackjack/${gameId.value}/${action}`)
    await fetchGameState() // Fetch updated state after action
  } catch (error: any) {
    console.error(`Error performing ${action} action:`, error)
    errorMessage.value = error.response?.data?.message || `Failed to ${action}.`
  } finally {
    isLoading.value = false
  }
}

function resetGame() {
  gameId.value = null
  gameState.value = null
  errorMessage.value = null
  betAmount.value = 10 // Reset bet to default
}

// --- Helper for Card Display ---
function getCardDisplay(card: CardDTO, isHidden: boolean = false) {
  if (isHidden) {
    return { rank: '?', symbol: '?', color: '#000' }
  }
  return {
    rank: card.rank,
    symbol: card.suit.symbol,
    color: card.suit.colorHex
  }
}

onMounted(() => {
  // Optionally, try to load an existing game if gameId is stored (e.g., in localStorage)
  // For simplicity, we'll just start fresh or wait for user to start.
})
</script>

<template>
  <div class="blackjack-container">
    <h1>Blackjack</h1>

    <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>

    <div v-if="!gameId || !gameState" class="game-setup">
      <h2>Start New Game</h2>
      <form @submit.prevent="startGame">
        <div class="form-group">
          <label for="bet-amount">Bet Amount:</label>
          <input
            id="bet-amount"
            v-model.number="betAmount"
            type="number"
            min="1"
            required
            :disabled="isLoading"
          />
        </div>
        <button type="submit" class="btn btn-primary" :disabled="isLoading">
          {{ isLoading ? 'Starting...' : 'Start Game' }}
        </button>
      </form>
    </div>

    <div v-else class="game-board">
      <div class="game-info">
        <p><strong>Game ID:</strong> {{ gameState.gameId }}</p>
        <p><strong>Your Money:</strong> ${{ gameState.playerMoney }}</p>
        <p><strong>Current Bet:</strong> ${{ gameState.currentBet }}</p>
        <p><strong>Status:</strong> {{ gameState.status }}</p>
        <p class="game-message">{{ gameState.message }}</p>
      </div>

      <div class="hands-container">
        <!-- Dealer's Hand -->
        <div class="hand dealer-hand">
          <h3>Dealer's Hand (Score: {{ gameState.dealerScore }})</h3>
          <div class="cards">
            <div
              v-for="(card, index) in gameState.dealerHand"
              :key="index"
              class="card"
              :class="{ hidden: isGameActive && index === 0 }"
            >
              <div
                class="card-content"
                :style="{
                  color: getCardDisplay(card, isGameActive && index === 0).color
                }"
              >
                <span class="card-rank">{{
                  getCardDisplay(card, isGameActive && index === 0).rank
                }}</span>
                <span class="card-symbol">{{
                  getCardDisplay(card, isGameActive && index === 0).symbol
                }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Player's Hand -->
        <div class="hand player-hand">
          <h3>Your Hand (Score: {{ gameState.playerScore }})</h3>
          <div class="cards">
            <div v-for="(card, index) in gameState.playerHand" :key="index" class="card">
              <div class="card-content" :style="{ color: getCardDisplay(card).color }">
                <span class="card-rank">{{ getCardDisplay(card).rank }}</span>
                <span class="card-symbol">{{ getCardDisplay(card).symbol }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="game-actions">
        <button @click="playerAction('hit')" :disabled="!isGameActive || isLoading" class="btn btn-primary">
          {{ isLoading ? 'Hitting...' : 'Hit' }}
        </button>
        <button @click="playerAction('stand')" :disabled="!isGameActive || isLoading" class="btn btn-secondary">
          {{ isLoading ? 'Standing...' : 'Stand' }}
        </button>
        <button @click="resetGame" class="btn btn-danger">New Game</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.blackjack-container {
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem;
  background-color: #f8f9fa;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

h1 {
  text-align: center;
  color: var(--color-text);
  margin-bottom: 2rem;
  border-bottom: 1px solid var(--color-border);
  padding-bottom: 0.5rem;
}

h2 {
  color: var(--color-text);
  margin-bottom: 1.5rem;
}

h3 {
  color: var(--color-text-soft);
  margin-bottom: 1rem;
}

.error-message {
  background-color: #f8d7da;
  color: #721c24;
  border: 1px solid #f5c6cb;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  text-align: center;
}

.game-setup,
.game-board {
  padding: 1.5rem;
  border-radius: 8px;
  background-color: #fff;
  border: 1px solid var(--color-border);
}

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: var(--color-text);
}

input[type='number'] {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
}

.game-info {
  background-color: #e9ecef;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 2rem;
  border: 1px solid var(--color-border);
}

.game-info p {
  margin: 0.5rem 0;
  color: var(--color-text);
}

.game-message {
  font-style: italic;
  font-weight: 500;
  color: var(--color-accent);
}

.hands-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  margin-bottom: 2rem;
}

.hand {
  background-color: #f1f3f5;
  padding: 1.5rem;
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

.cards {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-top: 1rem;
}

.card {
  width: 100px;
  height: 140px;
  background-color: #fff;
  border: 1px solid #ccc;
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
  font-family: 'Times New Roman', serif;
  font-weight: bold;
  position: relative;
}

.card.hidden {
  background-color: #343a40;
  color: #fff;
  border-color: #212529;
}

.card.hidden .card-content {
  visibility: hidden; /* Hide content but keep structure */
}

.card.hidden::after {
  content: '?';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 4rem;
  color: #fff;
}

.card-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  width: 100%;
}

.card-rank {
  font-size: 2.5rem;
  line-height: 1;
}

.card-symbol {
  font-size: 3rem;
  line-height: 1;
}

.game-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-top: 2rem;
}

.btn {
  padding: 0.75rem 1.5rem;
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

.btn-secondary {
  background-color: #6c757d;
}
.btn-secondary:hover:not(:disabled) {
  background-color: #5a6268;
}

.btn-danger {
  background-color: #dc3545;
}
.btn-danger:hover:not(:disabled) {
  background-color: #c82333;
}
</style>
