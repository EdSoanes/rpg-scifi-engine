import { Atom, useAtom } from 'jotai'
import {
  Stat,
  StatNumber,
  StatHelpText,
  StatLabel,
  StatArrow,
} from '@chakra-ui/react'
import React from 'react'
import { PropValue } from '../../lib/rpg-api/types'

export declare interface StatPanelProps {
  propName: string
  propNameAbbr: string
  propValueAtom: Atom<PropValue | null>
}

function StatPanel(props: StatPanelProps) {
  const [propValue] = useAtom(props.propValueAtom)
  const eq = (propValue?.value ?? 0) === (propValue?.baseValue ?? 0)
  const inc = !eq && (propValue?.value ?? 0) > (propValue?.baseValue ?? 0)
  const dec = !eq && (propValue?.value ?? 0) < (propValue?.baseValue ?? 0)

  return (
    <Stat m={4} p={4} border="1px" borderRadius={4} borderColor={'lightgray'}>
      <StatLabel>{props.propNameAbbr}</StatLabel>
      <StatNumber>{propValue?.value ?? 0}</StatNumber>
      <StatHelpText>
        {inc && <StatArrow type="increase" />}
        {dec && <StatArrow type="decrease" />}
        {props.propName} {propValue?.baseValue ?? 0}
      </StatHelpText>
    </Stat>
  )
}

export default StatPanel
