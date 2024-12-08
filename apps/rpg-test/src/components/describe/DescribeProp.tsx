import { Dice } from '@lib/rpg-api/types'
import { Heading, HStack, Text } from '@chakra-ui/react'
import { diceValue } from './utils'

export interface DescribeDiceProps {
  name?: string | null
  dice?: Dice | null
  level: number
}

export default function DescribeProp(props: DescribeDiceProps) {
  const { name, dice, level } = props

  const value = diceValue(dice)
  const padLeft = `${level * 10}px`

  return (
    name &&
    value && (
      <HStack paddingLeft={padLeft}>
        <Heading size={'sm'}>{name}</Heading>
        <Text textStyle={'lg'}>{value}</Text>
      </HStack>
    )
  )
}
