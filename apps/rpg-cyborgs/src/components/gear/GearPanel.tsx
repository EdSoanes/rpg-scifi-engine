import { RpgEntity } from '@lib/rpg-api/types'
import { Box } from '@chakra-ui/react'

export declare interface GearPanelProps {
  item: RpgEntity
}

function GearPanel(props: GearPanelProps) {
  const { item } = props

  return <Box>{item.name}</Box>
}

export default GearPanel
