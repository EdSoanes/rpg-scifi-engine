import { useAtomValue } from 'jotai'
import { Code, Heading, Stack } from '@chakra-ui/react'
import React from 'react'
import { graphStateAtom } from '../atoms/graphState.atom'

function GraphStateBlock() {
  const graphState = useAtomValue(graphStateAtom)

  return (
    <Stack w={'100%'}>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        Graph State
      </Heading>
      <Code>{JSON.stringify(graphState, undefined, 2)}</Code>
    </Stack>
  )
}

export default GraphStateBlock
