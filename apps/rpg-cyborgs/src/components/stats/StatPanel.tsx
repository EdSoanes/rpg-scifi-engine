import { PropValue } from '@lib/rpg-api/cyborg-types'
import PropertyValue from '@components/prop/property-value'
import { overridePropValue } from '@app/thunks'
import { useAppDispatch } from '@app/hooks'

export declare interface StatPanelProps {
  name: string
  abbreviatedName: string
  prop: string
  propValue?: PropValue
}

function StatPanel(props: StatPanelProps) {
  const { name, prop, propValue } = props
  const dispatch = useAppDispatch()

  const onPropValueChanged = async (
    value: number,
    propValue?: PropValue
  ): Promise<void> => {
    if (propValue?.ownerId && name) {
      console.log(`Updating propValue ${name}`, value)
      await dispatch(
        overridePropValue({
          propRef: { entityId: propValue.id, prop: prop },
          overrideValue: value,
        })
      )
    }
  }

  return (
    propValue && (
      <PropertyValue
        name={name}
        entityId={propValue.id}
        prop={prop}
        value={propValue.value}
        baseValue={propValue.baseValue}
        originalBaseValue={propValue.originalBaseValue}
        onPropValueChanged={async (value) =>
          await onPropValueChanged(value, propValue)
        }
      />
    )
  )
}

export default StatPanel
