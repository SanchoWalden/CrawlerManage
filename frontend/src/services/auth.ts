import type { AuthResponse, AuthenticatedUser } from '@/types'

const LOCAL_STORAGE_KEY = 'school-manage-auth'

export const auth = {
  token: '',
  expiresAt: '',
  user: null as AuthenticatedUser | null,
}

export function isAuthenticated(): boolean {
  if (!auth.token || !auth.expiresAt) return false
  const expires = Date.parse(auth.expiresAt)
  return Number.isFinite(expires) && expires > Date.now()
}

export function clearAuth(): void {
  auth.token = ''
  auth.expiresAt = ''
  auth.user = null
  localStorage.removeItem(LOCAL_STORAGE_KEY)
}

export function applyAuth(response: AuthResponse): void {
  auth.token = response.token
  auth.expiresAt = response.expiresAt
  auth.user = response.user
  localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(response))
}

export async function restoreAuth(): Promise<void> {
  try {
    const raw = localStorage.getItem(LOCAL_STORAGE_KEY)
    if (!raw) return
    const parsed = JSON.parse(raw) as AuthResponse
    if (!parsed.token || !parsed.expiresAt) {
      clearAuth()
      return
    }
    auth.token = parsed.token
    auth.expiresAt = parsed.expiresAt
    auth.user = parsed.user
  } catch {
    clearAuth()
  }
}

export function logout(): void {
  clearAuth()
}

export async function authorizedFetch(input: string, init: RequestInit = {}): Promise<Response> {
  if (!isAuthenticated()) {
    throw new Error('UNAUTHORIZED')
  }

  const headers = new Headers(init.headers ?? {})
  headers.set('Authorization', `Bearer ${auth.token}`)

  const response = await fetch(input, { ...init, headers })

  if (response.status === 401 || response.status === 403) {
    logout()
    throw new Error('UNAUTHORIZED')
  }

  return response
}