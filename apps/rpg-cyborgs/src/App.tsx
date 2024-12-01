import React from 'react'
import MainLayout from './layout/mainLayout'
import CharacterSheet from './pages/CharacterSheet'
import { ChakraProvider, defaultSystem } from '@chakra-ui/react'
import { ColorModeSwitcher } from './ColorModeSwitcher'
//const theme = extendTheme(baseTheme)

// 2. Add your color mode config

function App() {
  return (
    <ChakraProvider value={defaultSystem}>
      <MainLayout>
        <ColorModeSwitcher justifySelf="flex-end" />
        <CharacterSheet />
      </MainLayout>
    </ChakraProvider>
  )
}

export default App

