import React from 'react'
import { Provider } from 'react-redux'
import { Flex } from '@chakra-ui/react'
import Header from './header'
import { store } from '../app/store'

export default function MainLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <Provider store={store}>
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
