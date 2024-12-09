import { selectPlayerCharacter } from '@app/graphState/graphSelectors'
import { useAppSelector } from '@app/hooks'
import { Flex, Box, Heading } from '@chakra-ui/react'

import { LoadGraphButton } from '@components/graph'
import { Avatar } from '@components/ui/avatar'

const Header = () => {
  const playerCharacter = useAppSelector(selectPlayerCharacter)
  return (
    <Flex
      width={'100%'}
      backgroundColor={'cyan.900'}
      color={'white'}
      minH={'100px'}
      alignItems={'center'}
    >
      <Flex
        margin={'0 auto'}
        width={'100%'}
        maxW={{ xl: '1200px' }}
        justifyContent="space-between"
        alignItems={'center'}
      >
        <Box justifyContent={'left'}>
          <Avatar size="2xl" name="Sage" src="https://bit.ly/sage-adebayo" />
        </Box>
        <Box>
          <Heading color={'white'} as={'h1'} size={'xl'} margin={'0 auto'}>
            {playerCharacter?.name ?? '-'}
          </Heading>
        </Box>
        <Box justifyContent={'right'}>
          <LoadGraphButton />
        </Box>
      </Flex>
    </Flex>
  )
}

export default Header
