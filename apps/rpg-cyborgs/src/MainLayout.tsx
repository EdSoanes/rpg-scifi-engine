import React from 'react'
import { Provider } from 'react-redux'
import { Flex } from '@chakra-ui/react'
import { store } from '@app/store'

export default function MainLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <Provider store={store}>
      <Flex direction="column" align="center" width={'100%'} m="0 auto">
        {children}
      </Flex>
    </Provider>
  )
}
