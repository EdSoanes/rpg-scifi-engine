import React from 'react'
import { RpgEntity } from '../../lib/rpg-api/types'
import { StackItem } from '@chakra-ui/react'

export declare interface GearPanelProps {
  item: RpgEntity
}

function GearPanel(props: GearPanelProps) {
  const { item } = props

  return <StackItem>{item.name}</StackItem>
}

export default GearPanel
