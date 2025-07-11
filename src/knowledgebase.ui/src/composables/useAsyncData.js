import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useNotificationStore } from '@/stores/notification'
import { logger, errorHandler } from '@/utils/logger'

export function useAsyncData(fetchFn, options = {}) {
  const {
    immediate = true,
    resetOnExecute = true,
    shallow = false,
    onError = null,
    onSuccess = null,
    transform = null,
    cache = false,
    cacheKey = null,
    retry = 0,
    retryDelay = 1000,
    timeout = 0
  } = options

  const data = ref(null)
  const pending = ref(false)
  const error = ref(null)
  const retryCount = ref(0)
  const cacheMap = cache ? new Map() : null
  const notificationStore = useNotificationStore()

  const execute = async (...args) => {
    if (resetOnExecute) {
      data.value = null
      error.value = null
    }

    const key = cacheKey || JSON.stringify(args)
    if (cache && cacheMap.has(key)) {
      data.value = cacheMap.get(key)
      return data.value
    }

    pending.value = true
    
    try {
      let result
      
      if (timeout > 0) {
        result = await Promise.race([
          fetchFn(...args),
          new Promise((_, reject) => 
            setTimeout(() => reject(new Error('Request timeout')), timeout)
          )
        ])
      } else {
        result = await fetchFn(...args)
      }

      if (transform) {
        result = transform(result)
      }

      data.value = result
      error.value = null
      retryCount.value = 0

      if (cache) {
        cacheMap.set(key, result)
      }

      if (onSuccess) {
        onSuccess(result)
      }

      return result
    } catch (err) {
      error.value = err
      
      if (retryCount.value < retry) {
        retryCount.value++
        logger.warn(`Retrying request (${retryCount.value}/${retry})`, { error: err.message })
        
        await new Promise(resolve => setTimeout(resolve, retryDelay))
        return execute(...args)
      }

      errorHandler.captureException(err, {
        context: 'useAsyncData',
        args: args.slice(0, 3)
      })

      if (onError) {
        onError(err)
      } else {
        notificationStore.error('请求失败', err.message)
      }

      throw err
    } finally {
      pending.value = false
    }
  }

  const refresh = () => execute()

  const clear = () => {
    data.value = null
    error.value = null
    pending.value = false
    retryCount.value = 0
  }

  const clearCache = () => {
    if (cacheMap) {
      cacheMap.clear()
    }
  }

  if (immediate) {
    onMounted(() => execute())
  }

  return {
    data: shallow ? data : computed(() => data.value),
    pending: computed(() => pending.value),
    error: computed(() => error.value),
    execute,
    refresh,
    clear,
    clearCache,
    retryCount: computed(() => retryCount.value)
  }
}

export function useAsyncState(initialState, asyncFn, options = {}) {
  const state = ref(initialState)
  const { execute, pending, error } = useAsyncData(asyncFn, {
    ...options,
    onSuccess: (data) => {
      state.value = data
      if (options.onSuccess) {
        options.onSuccess(data)
      }
    }
  })

  const setState = (newState) => {
    state.value = newState
  }

  return {
    state,
    setState,
    execute,
    pending,
    error
  }
}

export function useInfiniteScroll(fetchFn, options = {}) {
  const {
    pageSize = 10,
    initialPage = 1,
    transform = null,
    hasMore = (data) => data && data.length === pageSize
  } = options

  const items = ref([])
  const currentPage = ref(initialPage)
  const loading = ref(false)
  const error = ref(null)
  const finished = ref(false)

  const load = async () => {
    if (loading.value || finished.value) return

    loading.value = true
    error.value = null

    try {
      const result = await fetchFn(currentPage.value, pageSize)
      const newItems = transform ? transform(result) : result

      if (currentPage.value === initialPage) {
        items.value = newItems
      } else {
        items.value = [...items.value, ...newItems]
      }

      if (!hasMore(newItems)) {
        finished.value = true
      } else {
        currentPage.value++
      }
    } catch (err) {
      error.value = err
      errorHandler.captureException(err, {
        context: 'useInfiniteScroll',
        page: currentPage.value
      })
    } finally {
      loading.value = false
    }
  }

  const refresh = async () => {
    currentPage.value = initialPage
    finished.value = false
    items.value = []
    await load()
  }

  const loadMore = () => {
    if (!loading.value && !finished.value) {
      load()
    }
  }

  return {
    items,
    loading,
    error,
    finished,
    load,
    refresh,
    loadMore
  }
}

export function usePolling(fetchFn, interval = 5000, options = {}) {
  const { immediate = true, enabled = true } = options
  const { data, pending, error, execute } = useAsyncData(fetchFn, { immediate })
  
  let timer = null
  const isPolling = ref(false)

  const start = () => {
    if (timer) return
    
    isPolling.value = true
    timer = setInterval(async () => {
      if (!document.hidden) {
        await execute()
      }
    }, interval)
  }

  const stop = () => {
    if (timer) {
      clearInterval(timer)
      timer = null
      isPolling.value = false
    }
  }

  const toggle = () => {
    if (isPolling.value) {
      stop()
    } else {
      start()
    }
  }

  watch(() => enabled, (newEnabled) => {
    if (newEnabled) {
      start()
    } else {
      stop()
    }
  })

  if (immediate && enabled) {
    onMounted(start)
  }

  onUnmounted(stop)

  return {
    data,
    pending,
    error,
    execute,
    isPolling,
    start,
    stop,
    toggle
  }
}