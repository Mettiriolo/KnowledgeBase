export const getPlainText = (html) => {
  if (!html) return ''

  const div = document.createElement('div')
  div.innerHTML = html
  return div.textContent || div.innerText || ''
}

export const truncateText = (text, maxLength = 150) => {
  if (!text) return ''

  const plainText = typeof text === 'string' ? text : getPlainText(text)
  return plainText.length > maxLength
    ? plainText.substring(0, maxLength) + '...'
    : plainText
}

export const getWordCount = (text) => {
  if (!text) return 0

  const plainText = typeof text === 'string' ? text : getPlainText(text)
  return plainText.length
}

export const getReadingTime = (text, wordsPerMinute = 200) => {
  const wordCount = getWordCount(text)
  return Math.ceil(wordCount / wordsPerMinute)
}

export const highlightText = (text, keyword) => {
  if (!text || !keyword) return text

  const regex = new RegExp(`(${keyword})`, 'gi')
  return text.replace(regex, '<mark class="bg-yellow-200">$1</mark>')
}

export const calculateReadingTime = (text) => {
  const wordCount = getWordCount(text)
  return Math.ceil(wordCount / 200)
}

export const getCharacterCount = (text) => {
  const plainText = typeof text === 'string' ? text : getPlainText(text)
  return plainText.length
}
