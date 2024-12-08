import { createSystem, defaultConfig, defineConfig } from '@chakra-ui/react'
import { headingRecipe } from './heading.recipe'

const cyborgConfig = defineConfig({
  globalCss: {
    html: {
      fontFamily: 'Roboto Condensed',
      backgroundColor: '#eee',
    },
    body: {
      fontFamily: 'Roboto Condensed',
    },
    h2: {
      fontFamily: 'Roboto Condensed',
    },
    h3: {
      fontFamily: 'Roboto Condensed',
    },
  },
  theme: {
    recipes: {
      heading: headingRecipe,
    },
  },
})

export const system = createSystem(defaultConfig, cyborgConfig)
