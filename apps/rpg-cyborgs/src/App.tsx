import React from 'react'
import MainLayout from './layout/mainLayout'
import CharacterSheet from './pages/CharacterSheet'
import { ChakraProvider, extendTheme } from '@chakra-ui/react'
import { ColorModeSwitcher } from './ColorModeSwitcher'
import { theme as baseTheme } from '@saas-ui/theme-glass'
//const theme = extendTheme(baseTheme)

// 2. Add your color mode config
const theme = extendTheme(
  {
    initialColorMode: 'dark',
    useSystemColorMode: false,
  },
  baseTheme
)

function App() {
  return (
    <ChakraProvider theme={theme}>
      <MainLayout>
        <ColorModeSwitcher justifySelf="flex-end" />
        <CharacterSheet />
      </MainLayout>
    </ChakraProvider>
  )
}

export default App
