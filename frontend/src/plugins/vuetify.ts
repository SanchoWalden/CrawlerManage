import 'vuetify/styles'
import { md3 } from 'vuetify/blueprints'
import { createVuetify } from 'vuetify'
import { aliases, mdi } from 'vuetify/iconsets/mdi'
import '@mdi/font/css/materialdesignicons.css'

const lightPalette = {
  dark: false,
  colors: {
    primary: '#1867C0',
    secondary: '#5CBBF6',
    accent: '#4CAF50',
    warning: '#FB8C00',
    error: '#FF5252',
    info: '#2196F3',
    success: '#4CAF50',
    background: '#F5F7FA',
  },
}

export const vuetify = createVuetify({
  blueprint: md3,
  theme: {
    defaultTheme: 'lightPalette',
    themes: {
      lightPalette,
    },
  },
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: { mdi },
  },
})
