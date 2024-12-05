import { Box, Heading, Stack } from '@chakra-ui/react'
import { selectGraphState } from '../../app/graphState/graphSelectors'
import { useSelector } from 'react-redux'
import ReactJson from 'react-json-view'

function GraphStateBlock() {
  const graphState = useSelector(selectGraphState)

  return (
    <Stack w={'100%'}>
      <Heading as="h3" size="lg" paddingBottom={4} paddingTop={10}>
        Graph State
      </Heading>
      <Box h="300px" w="full">
        {graphState && <ReactJson src={graphState} collapsed={true} />}
      </Box>
    </Stack>
  )
}

export default GraphStateBlock
