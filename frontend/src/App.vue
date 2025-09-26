<script setup lang="ts">
import { RouterLink, RouterView, useRoute } from 'vue-router'
import { ref, watch } from 'vue'

const subtitles = [
  'now with 24% more line coverage and 35% more branch coverage!',
  'I LOVE GAMBLING!!!!!!!1!',
  'have you heard of our lord and savior switch expressions?',
  'https://en.wikipedia.org/wiki/Gamma_function',
  'you should play library of ruina',
  'you want a +4 mult with that?',
]

const subtitle = ref('')
const route = useRoute()

watch(
  () => route.path,
  () => {
    subtitle.value = subtitles[Math.floor(Math.random() * subtitles.length)]
  },
  {
    immediate: true,
  },
)
</script>

<template>
  <header>
    <div class="wrapper">
      <RouterLink to="/" class="title-link">
        <h1><del>Balatro</del> Blackjack</h1>
      </RouterLink>
      <h2>{{ subtitle }}</h2>

      <nav>
        <RouterLink to="/suits">Suits</RouterLink>
        <RouterLink to="/cards">Cards</RouterLink>
        <RouterLink to="/deck">Deck</RouterLink>
        <RouterLink to="/blackjack">Blackjack</RouterLink>
        <RouterLink to="/admin">Admin</RouterLink>
      </nav>
    </div>
  </header>

  <main>
    <RouterView />
  </main>
</template>

<style>
:root {
  --color-background: #ffffff;
  --color-text: #2c3e50;
  --color-text-soft: #8492a6;
  --color-border: #e0e6ed;
  --color-accent: #e53935;
  --color-accent-hover: #c62828;
}

body {
  background-color: var(--color-background);
  color: var(--color-text);

  /* force single-column layout */
  @media (min-width: 1024px) {
    #app {
      grid-template-columns: 1fr;
    }
  }
}
</style>

<style scoped>
.title-link {
  text-decoration: none;
}

header {
  text-align: center;
  padding: 2rem 0;
  border-bottom: 1px solid var(--color-border);
}

h1 {
  font-size: 2.5rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: var(--color-text);
}

h1 del {
  color: var(--color-accent);
  text-decoration-color: var(--color-text-soft);
}

h2 {
  font-size: 1.1rem;
  color: var(--color-text-soft);
  margin-bottom: 2rem;
}

nav {
  width: 100%;
  font-size: 1rem;
  text-align: center;
}

nav a {
  display: inline-block;
  padding: 0.5rem 1rem;
  border-left: 1px solid var(--color-border);
  color: var(--color-text);
  text-decoration: none;
  transition: color 0.2s ease-in-out;
}

nav a:hover {
  color: var(--color-accent-hover);
}

nav a.router-link-exact-active {
  color: var(--color-accent);
  font-weight: 600;
}

nav a.router-link-exact-active:hover {
  background-color: transparent;
}

nav a:first-of-type {
  border: 0;
}

main {
  padding: 2rem;
}
</style>
