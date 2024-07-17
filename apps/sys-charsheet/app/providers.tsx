'use client'

import { ChakraProvider } from '@chakra-ui/react'
import { Provider, createStore } from "jotai";  

export function Providers({ children }: { children: React.ReactNode }) {
  return <ChakraProvider><Provider>{children}</Provider></ChakraProvider>
}