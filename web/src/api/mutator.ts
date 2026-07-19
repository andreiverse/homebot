export const customFetch = async <T>(
  url: string,
  options: RequestInit
): Promise<T> => {
  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(`HTTP ${response.status}: ${text || response.statusText}`)
  }

  if (response.status === 204) {
    return {
      data: {} as any,
      status: response.status,
      headers: response.headers,
    } as T
  }

  const data = await response.json()
  return {
    data,
    status: response.status,
    headers: response.headers,
  } as T
}

export default customFetch
