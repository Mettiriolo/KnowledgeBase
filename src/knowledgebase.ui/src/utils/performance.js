export const debounce = (fn, delay = 300) => {
  let timeoutId
  return (...args) => {
    clearTimeout(timeoutId)
    timeoutId = setTimeout(() => fn.apply(this, args), delay)
  }
}

export const throttle = (fn, limit = 100) => {
  let inThrottle
  return (...args) => {
    if (!inThrottle) {
      fn.apply(this, args)
      inThrottle = true
      setTimeout(() => inThrottle = false, limit)
    }
  }
}

export const memoize = (fn, keyGenerator) => {
  const cache = new Map()
  
  return (...args) => {
    const key = keyGenerator ? keyGenerator(args) : JSON.stringify(args)
    
    if (cache.has(key)) {
      return cache.get(key)
    }
    
    const result = fn.apply(this, args)
    cache.set(key, result)
    return result
  }
}

export const createCache = (ttl = 5 * 60 * 1000) => {
  const cache = new Map()
  
  return {
    get: (key) => {
      const item = cache.get(key)
      if (!item) return null
      
      if (Date.now() - item.timestamp > ttl) {
        cache.delete(key)
        return null
      }
      
      return item.value
    },
    
    set: (key, value) => {
      cache.set(key, {
        value,
        timestamp: Date.now()
      })
    },
    
    delete: (key) => {
      cache.delete(key)
    },
    
    clear: () => {
      cache.clear()
    },
    
    size: () => cache.size
  }
}

export const lazy = (importFn) => {
  let component = null
  let promise = null
  
  return () => {
    if (component) return component
    
    if (!promise) {
      promise = importFn().then(module => {
        component = module.default || module
        return component
      })
    }
    
    return promise
  }
}

export const intersection = (callback, options = {}) => {
  const defaultOptions = {
    threshold: 0.1,
    rootMargin: '0px'
  }
  
  const observer = new IntersectionObserver(callback, { ...defaultOptions, ...options })
  
  return {
    observe: (element) => observer.observe(element),
    unobserve: (element) => observer.unobserve(element),
    disconnect: () => observer.disconnect()
  }
}

export const virtualScroll = (container, items, renderItem, itemHeight = 40) => {
  const containerHeight = container.clientHeight
  const visibleCount = Math.ceil(containerHeight / itemHeight)
  const buffer = 5
  
  let startIndex = 0
  let endIndex = visibleCount + buffer
  
  const render = () => {
    const fragment = document.createDocumentFragment()
    
    for (let i = startIndex; i < Math.min(endIndex, items.length); i++) {
      const item = renderItem(items[i], i)
      item.style.position = 'absolute'
      item.style.top = `${i * itemHeight}px`
      fragment.appendChild(item)
    }
    
    container.innerHTML = ''
    container.appendChild(fragment)
    container.style.height = `${items.length * itemHeight}px`
  }
  
  const handleScroll = () => {
    const scrollTop = container.scrollTop
    const newStartIndex = Math.floor(scrollTop / itemHeight)
    const newEndIndex = newStartIndex + visibleCount + buffer
    
    if (newStartIndex !== startIndex || newEndIndex !== endIndex) {
      startIndex = Math.max(0, newStartIndex - buffer)
      endIndex = Math.min(items.length, newEndIndex)
      render()
    }
  }
  
  container.addEventListener('scroll', throttle(handleScroll, 16))
  render()
  
  return {
    update: (newItems) => {
      items = newItems
      render()
    },
    destroy: () => {
      container.removeEventListener('scroll', handleScroll)
    }
  }
}

export const preloader = () => {
  const cache = new Map()
  
  return {
    preload: async (url) => {
      if (cache.has(url)) return cache.get(url)
      
      try {
        const response = await fetch(url)
        const data = await response.json()
        cache.set(url, data)
        return data
      } catch (error) {
        console.error('Preload failed:', error)
        return null
      }
    },
    
    get: (url) => cache.get(url),
    clear: () => cache.clear()
  }
}

export const measurePerformance = (name, fn) => {
  return async (...args) => {
    const start = performance.now()
    const result = await fn(...args)
    const end = performance.now()
    
    console.log(`${name} took ${end - start} milliseconds`)
    return result
  }
}

export const batchOperations = (operations, batchSize = 5) => {
  const batches = []
  
  for (let i = 0; i < operations.length; i += batchSize) {
    batches.push(operations.slice(i, i + batchSize))
  }
  
  return batches.reduce(async (promise, batch) => {
    await promise
    return Promise.all(batch.map(op => op()))
  }, Promise.resolve())
}

export const resourceMonitor = () => {
  const resources = new Map()
  
  return {
    track: (name, resource) => {
      resources.set(name, resource)
    },
    
    cleanup: () => {
      resources.forEach((resource, name) => {
        if (resource.cleanup) {
          resource.cleanup()
        }
        console.log(`Cleaned up resource: ${name}`)
      })
      resources.clear()
    },
    
    get: (name) => resources.get(name),
    list: () => Array.from(resources.keys())
  }
}

export const idleCallback = (callback, timeout = 5000) => {
  if (window.requestIdleCallback) {
    return window.requestIdleCallback(callback, { timeout })
  } else {
    return setTimeout(callback, 16)
  }
}

export const cancelIdleCallback = (id) => {
  if (window.cancelIdleCallback) {
    window.cancelIdleCallback(id)
  } else {
    clearTimeout(id)
  }
}