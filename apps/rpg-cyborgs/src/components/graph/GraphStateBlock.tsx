import { Code, Heading, Stack } from '@chakra-ui/react'
import { selectGraphState } from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'

function GraphStateBlock() {
  const graphState = useSelector(selectGraphState)

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
