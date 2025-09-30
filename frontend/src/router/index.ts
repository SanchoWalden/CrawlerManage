import { createRouter, createWebHistory } from 'vue-router'
import { isAuthenticated } from '@/services/auth'
import LoginView from '@/views/auth/LoginView.vue'
import DashboardView from '@/views/dashboard/DashboardView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/dashboard',
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { requiresAuth: false },
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: DashboardView,
      meta: { requiresAuth: true },
    },
  ],
})

router.beforeEach((to, from, next) => {
  const requiresAuth = to.meta.requiresAuth !== false

  if (requiresAuth && !isAuthenticated()) {
    next({ name: 'login' })
  } else if (to.name === 'login' && isAuthenticated()) {
    next({ name: 'dashboard' })
  } else {
    next()
  }
})

export default router