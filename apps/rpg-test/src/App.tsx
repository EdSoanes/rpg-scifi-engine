'use client'

import { ChakraProvider } from '@chakra-ui/react'
import MainLayout from '@layout/MainLayout'
import CharacterSheet from '@pages/CharacterSheet'
import Header from '@layout/Header'
import '@fontsource/roboto-condensed' // Defaults to weight 400
import '@fontsource/roboto-condensed/400.css' // Specify weight
import '@fontsource/roboto-condensed/400-italic.css' // Specify weight and style
import { system } from '@components/theme/theme'
//const theme = extendTheme(baseTheme)

// 2. Add your color mode config

function App() {
  return (
    <ChakraProvider value={system}>
      <MainLayout>
        <Header />
        <CharacterSheet />
      </MainLayout>
    </ChakraProvider>
  )
}

export default App
