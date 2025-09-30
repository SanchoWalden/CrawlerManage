export interface ScrapedItem {
  id: number
  title: string
  url: string
  source?: string | null
  summary?: string | null
  collectedAt: string
}

export interface PaginatedResponse {
  total: number
  page: number
  pageSize: number
  items: ScrapedItem[]
}

export interface AuthenticatedUser {
  id: string
  userName?: string | null
  email?: string | null
  displayName?: string | null
  roles: string[]
}

export interface AuthResponse {
  token: string
  expiresAt: string
  user: AuthenticatedUser
}

export type SnackbarColor = 'success' | 'error' | 'info' | 'warning'