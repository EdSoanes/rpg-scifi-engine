import React from 'react'

import { Box, Flex } from '@chakra-ui/react'

import { PiCross, PiHamburger } from 'react-icons/pi'
import { LoadGraphButton } from '../components/graph'

export declare interface HeaderProps {
  children?: React.ReactNode // best, accepts everything React can render
  style?: React.CSSProperties // to pass through style props
}

const Header = (props: HeaderProps) => {
  const [show, setShow] = React.useState(false)
  const toggleMenu = () => setShow(!show)

  return (
    <Flex
      as="nav"
      align="center"
      justify="space-between"
      wrap="wrap"
      w="100%"
      mb={8}
      p={8}
      bg={['primary.500', 'primary.500', 'transparent', 'transparent']}
      color={['white', 'white', 'primary.700', 'primary.700']}
      {...props}
    >
      <Flex align="center"></Flex>

      <Box display={{ base: 'block', md: 'none' }} onClick={toggleMenu}>
        {show ? <PiCross /> : <PiHamburger />}
      </Box>

      <Box
        display={{ base: show ? 'block' : 'none', md: 'block' }}
        flexBasis={{ base: '100%', md: 'auto' }}
      >
        <Flex
          align={['center', 'center', 'center', 'center']}
          justify={['center', 'space-between', 'flex-end', 'flex-end']}
          direction={['column', 'row', 'row', 'row']}
          pt={[4, 4, 0, 0]}
        >
          <LoadGraphButton />
        </Flex>
      </Box>
    </Flex>
  )
}

export default Header
