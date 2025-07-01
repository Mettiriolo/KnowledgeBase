import { format, formatDistanceToNow, isToday, isYesterday } from 'date-fns'
import { zhCN } from 'date-fns/locale'

export const formatDate = (date) => {
  if (!date) return ''

  const dateObj = new Date(date)

  if (isToday(dateObj)) {
    return `今天 ${format(dateObj, 'HH:mm')}`
  } else if (isYesterday(dateObj)) {
    return `昨天 ${format(dateObj, 'HH:mm')}`
  } else {
    return format(dateObj, 'yyyy-MM-dd HH:mm')
  }
}

export const formatRelativeTime = (date) => {
  if (!date) return ''

  return formatDistanceToNow(new Date(date), {
    addSuffix: true,
    locale: zhCN
  })
}

export const formatFullDate = (date) => {
  if (!date) return ''

  return format(new Date(date), 'yyyy年MM月dd日 HH:mm:ss')
}
