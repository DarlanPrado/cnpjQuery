import createClient from 'openapi-fetch'
import type { paths } from './schema.d.ts'

export const apiClient = createClient<paths>({
  baseUrl: 'https://localhost:7040',
})
