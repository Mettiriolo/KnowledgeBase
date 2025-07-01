import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useAppStore } from '@/stores/app'

// 路由懒加载
const Login = () => import('@/views/Login.vue')
const Dashboard = () => import('@/views/Dashboard.vue')
const Notes = () => import('@/views/Notes.vue')
const NoteDetail = () => import('@/views/NoteDetail.vue')
const NoteEditor = () => import('@/views/NoteEditor.vue')
const Search = () => import('@/views/Search.vue')
const NotFound = () => import('@/views/NotFound.vue')

const routes = [
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: {
      requiresAuth: false,
      title: '登录 - AI知识库'
    }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: Dashboard,
    meta: {
      requiresAuth: true,
      title: '仪表板 - AI知识库'
    }
  },
  {
    path: '/notes',
    name: 'Notes',
    component: Notes,
    meta: {
      requiresAuth: true,
      title: '我的笔记 - AI知识库'
    }
  },
  {
    path: '/notes/create',
    name: 'CreateNote',
    component: NoteEditor,
    meta: {
      requiresAuth: true,
      title: '创建笔记 - AI知识库'
    }
  },
  {
    path: '/notes/:id',
    name: 'NoteDetail',
    component: NoteDetail,
    meta: {
      requiresAuth: true,
      title: '笔记详情 - AI知识库'
    }
  },
  {
    path: '/notes/:id/edit',
    name: 'EditNote',
    component: NoteEditor,
    meta: {
      requiresAuth: true,
      title: '编辑笔记 - AI知识库'
    }
  },
  {
    path: '/search',
    name: 'Search',
    component: Search,
    meta: {
      requiresAuth: true,
      title: '智能搜索 - AI知识库'
    }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: NotFound,
    meta: {
      title: '页面未找到 - AI知识库'
    }
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    } else if (to.hash) {
      return { el: to.hash, behavior: 'smooth' }
    } else {
      return { top: 0 }
    }
  }
})

// 路由守卫
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  const appStore = useAppStore()

  // 设置页面标题
  document.title = to.meta.title || 'AI知识库'

  // 显示全局加载
  appStore.setGlobalLoading(true)

  // 检查认证状态
  if (to.meta.requiresAuth !== false) {
    if (!authStore.isAuthenticated) {
      // 保存原始目标路由
      const redirectUrl = to.fullPath
      next({
        path: '/login',
        query: { redirect: redirectUrl !== '/dashboard' ? redirectUrl : undefined }
      })
      appStore.setGlobalLoading(false)
      return
    }
  } else if (to.path === '/login' && authStore.isAuthenticated) {
    // 已登录用户访问登录页，重定向到首页
    next('/dashboard')
    appStore.setGlobalLoading(false)
    return
  }

  next()
})

router.afterEach(() => {
  const appStore = useAppStore()
  appStore.setGlobalLoading(false)
})

export default router
