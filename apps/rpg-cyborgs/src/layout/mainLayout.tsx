import React from 'react'
import { Provider } from 'jotai'
import { Flex } from '@chakra-ui/react'
import Header from './header'

export default function MainLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <Provider>
      <Flex
        direction="column"
        align="center"
        maxW={{ xl: '1200px' }}
        m="0 auto"
      >
        <Header></Header>
        {children}
      </Flex>
    </Provider>
  )
}
