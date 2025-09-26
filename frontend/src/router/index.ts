import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/suits',
      name: 'suits',
      component: () => import('../views/SuitsView.vue'),
    },
    {
      path: '/cards',
      name: 'cards',
      component: () => import('../views/CardsView.vue'),
    },
    {
      path: '/deck',
      name: 'deck',
      component: () => import('../views/DeckView.vue'),
    },
    {
      path: '/blackjack',
      name: 'blackjack',
      component: () => import('../views/BlackjackView.vue'),
    },
  ],
})

export default router
